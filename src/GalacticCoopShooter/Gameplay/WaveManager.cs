using GalacticCoopShooter.Entities;
using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public sealed class WaveManager
{
    private readonly Rectangle _movementBounds;
    private readonly Random _random;
    private readonly List<Enemy> _enemies = [];

    private float _direction = 1f;
    private float _groupSpeed;
    private float _eggChancePerSecond;

    public WaveManager(Rectangle movementBounds, Random random)
    {
        _movementBounds = movementBounds;
        _random = random;
    }

    public int CurrentWave { get; private set; }

    public IReadOnlyList<Enemy> Enemies => _enemies;

    public bool IsWaveCleared => _enemies.Count == 0;

    public void StartWave(int waveNumber)
    {
        CurrentWave = waveNumber;
        _direction = 1f;
        _groupSpeed = 34f + ((waveNumber - 1) * 14f);
        _eggChancePerSecond = 0.48f + ((waveNumber - 1) * 0.18f);

        SpawnFormation(waveNumber);
    }

    public void Update(float deltaTime, List<Projectile> enemyEggs)
    {
        if (_enemies.Count == 0)
        {
            return;
        }

        MoveFormation(deltaTime);
        SpawnEnemyEggs(deltaTime, enemyEggs);
    }

    public void PruneInactive()
    {
        _enemies.RemoveAll(enemy => !enemy.IsActive);
    }

    private void SpawnFormation(int waveNumber)
    {
        _enemies.Clear();

        var rows = 3 + Math.Min(2, waveNumber);
        const int columns = 9;

        const float spacingX = 74f;
        const float spacingY = 52f;

        var formationWidth = (columns - 1) * spacingX;
        var startX = (_movementBounds.Width - formationWidth) * 0.5f;
        const float startY = 86f;

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var enemyType = ResolveEnemyType(waveNumber, row, column);
                var enemyPosition = new Vector2(startX + (column * spacingX), startY + (row * spacingY));

                var enemy = new Enemy(enemyPosition, enemyType, column);
                enemy.ResetShotCooldown(_random);
                _enemies.Add(enemy);
            }
        }
    }

    private static EnemyType ResolveEnemyType(int waveNumber, int row, int column)
    {
        if (waveNumber >= 3 && (row + column) % 4 == 0)
        {
            return EnemyType.Rapid;
        }

        if (waveNumber >= 2 && (row + column) % 3 == 0)
        {
            return EnemyType.Tough;
        }

        return EnemyType.Normal;
    }

    private void MoveFormation(float deltaTime)
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;

        for (var i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            minX = MathF.Min(minX, enemy.Position.X);
            maxX = MathF.Max(maxX, enemy.Position.X + enemy.Size.X);
        }

        var deltaX = _direction * _groupSpeed * deltaTime;
        var projectedMinX = minX + deltaX;
        var projectedMaxX = maxX + deltaX;

        var wouldHitLeft = projectedMinX <= _movementBounds.Left + 16f;
        var wouldHitRight = projectedMaxX >= _movementBounds.Right - 16f;

        if (wouldHitLeft || wouldHitRight)
        {
            _direction *= -1f;

            for (var i = 0; i < _enemies.Count; i++)
            {
                _enemies[i].Move(new Vector2(0f, 24f));
            }

            return;
        }

        for (var i = 0; i < _enemies.Count; i++)
        {
            _enemies[i].Move(new Vector2(deltaX, 0f));
        }
    }

    private void SpawnEnemyEggs(float deltaTime, List<Projectile> enemyEggs)
    {
        var bottomShooters = new Dictionary<int, Enemy>();

        for (var i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];

            if (bottomShooters.TryGetValue(enemy.GridColumn, out var existing))
            {
                if (enemy.Position.Y > existing.Position.Y)
                {
                    bottomShooters[enemy.GridColumn] = enemy;
                }
            }
            else
            {
                bottomShooters[enemy.GridColumn] = enemy;
            }
        }

        foreach (var shooter in bottomShooters.Values)
        {
            shooter.ShotCooldown -= deltaTime;

            if (shooter.ShotCooldown > 0f)
            {
                continue;
            }

            var fireChance = _eggChancePerSecond * shooter.FireRateMultiplier * deltaTime;

            if (_random.NextSingle() > fireChance)
            {
                continue;
            }

            var eggPosition = new Vector2(shooter.Position.X + (shooter.Size.X * 0.5f) - 3f, shooter.Position.Y + shooter.Size.Y);
            var eggVelocity = new Vector2(0f, 210f + ((CurrentWave - 1) * 26f));

            enemyEggs.Add(new Projectile(eggPosition, eggVelocity, true, new Color(250, 232, 145)));
            shooter.ResetShotCooldown(_random);
        }
    }
}
