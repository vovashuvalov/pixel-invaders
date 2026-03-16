using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public enum EnemyType
{
    Green,
    Red,
    Blue,
    Yellow,
    Boss
}

public sealed class Enemy : Entity
{
    private readonly int _levelNumber;
    private readonly int _encounterNumber;
    private readonly int _spawnIndex;
    private readonly float _baseSpeed;
    private readonly float _horizontalSpeed;

    private float _attackCooldown;
    private float _secondaryCooldown;
    private float _elapsedTime;
    private float _targetX;
    private float _bossDirection = 1f;
    private bool _hasTarget;

    public Enemy(Vector2 position, EnemyType type, int levelNumber, int encounterNumber, int spawnIndex)
        : base(position, ResolveSize(type))
    {
        Type = type;
        _levelNumber = levelNumber;
        _encounterNumber = encounterNumber;
        _spawnIndex = spawnIndex;

        switch (type)
        {
            case EnemyType.Red:
                Health = 1;
                ScoreValue = 140;
                _baseSpeed = 120f + (levelNumber * 6f) + (encounterNumber * 3f);
                _horizontalSpeed = 170f;
                break;
            case EnemyType.Blue:
                Health = 2;
                ScoreValue = 190;
                _baseSpeed = 88f + (levelNumber * 5f) + (encounterNumber * 2f);
                _horizontalSpeed = 72f;
                break;
            case EnemyType.Yellow:
                Health = 2;
                ScoreValue = 240;
                _baseSpeed = 128f + (levelNumber * 6f) + (encounterNumber * 4f);
                _horizontalSpeed = 135f;
                break;
            case EnemyType.Boss:
                Health = 24 + (levelNumber * 8);
                ScoreValue = 2800 + (levelNumber * 500);
                _baseSpeed = 0f;
                _horizontalSpeed = 118f + (levelNumber * 8f);
                break;
            default:
                Health = 1;
                ScoreValue = 120;
                _baseSpeed = 92f + (levelNumber * 5f) + (encounterNumber * 3f);
                _horizontalSpeed = 18f;
                break;
        }

        ResetAttackTimers(new Random(levelNumber * 1000 + encounterNumber * 100 + spawnIndex * 13 + (int)type));
    }

    public EnemyType Type { get; }
    public int Health { get; private set; }
    public int ScoreValue { get; }
    public bool IsBoss => Type == EnemyType.Boss;
    public Vector2 Center => Position + (Size * 0.5f);

    public bool TakeDamage(int amount)
    {
        Health -= amount;

        if (Health > 0)
        {
            return false;
        }

        IsActive = false;
        return true;
    }

    public bool HasReachedEarth(Rectangle bounds)
    {
        return !IsBoss && Position.Y >= bounds.Bottom + 12f;
    }

    public void UpdateBehavior(float deltaTime, Vector2 playerCenter, Rectangle movementBounds, List<Projectile> enemyProjectiles, Random random)
    {
        if (!IsActive)
        {
            return;
        }

        _elapsedTime += deltaTime;

        switch (Type)
        {
            case EnemyType.Red:
                UpdateDiver(deltaTime, playerCenter);
                break;
            case EnemyType.Blue:
                UpdateShooter(deltaTime, enemyProjectiles, random);
                break;
            case EnemyType.Yellow:
                UpdateMiner(deltaTime, enemyProjectiles, random);
                break;
            case EnemyType.Boss:
                UpdateBoss(deltaTime, playerCenter, movementBounds, enemyProjectiles, random);
                break;
            default:
                Position += new Vector2(MathF.Sin((_elapsedTime + _spawnIndex) * 1.2f) * _horizontalSpeed * deltaTime, _baseSpeed * deltaTime);
                break;
        }
    }

    public void ResetAttackTimers(Random random)
    {
        _attackCooldown = Type switch
        {
            EnemyType.Blue => 0.8f + random.NextSingle() * 0.8f,
            EnemyType.Boss => 0.75f + random.NextSingle() * 0.35f,
            _ => 1.2f + random.NextSingle()
        };

        _secondaryCooldown = Type switch
        {
            EnemyType.Yellow => 1.0f + random.NextSingle() * 0.8f,
            EnemyType.Boss => 1.7f + random.NextSingle() * 0.8f,
            _ => 1.4f + random.NextSingle()
        };
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        var x = (int)Position.X;
        var y = (int)Position.Y;

        switch (Type)
        {
            case EnemyType.Red:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 14, y + 4, 10, 8), new Color(255, 238, 210));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 8, y + 12, 22, 10), new Color(232, 75, 80));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 4, y + 20, 30, 6), new Color(170, 35, 45));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 16, y + 26, 6, 4), new Color(255, 195, 125));
                break;
            case EnemyType.Blue:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 6, y + 10, 30, 16), new Color(105, 180, 255));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 12, y + 2, 18, 10), new Color(225, 240, 255));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 2, y + 14, 8, 6), new Color(52, 95, 170));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 32, y + 14, 8, 6), new Color(52, 95, 170));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 18, y + 26, 6, 6), new Color(255, 210, 120));
                break;
            case EnemyType.Yellow:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 7, y + 8, 26, 12), new Color(255, 218, 78));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 3, y + 14, 8, 8), new Color(255, 150, 55));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 29, y + 14, 8, 8), new Color(255, 150, 55));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 15, y + 2, 10, 8), new Color(255, 245, 210));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 12, y + 22, 14, 4), new Color(160, 85, 20));
                break;
            case EnemyType.Boss:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 26, y + 22, 96, 38), new Color(204, 70, 98));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 42, y + 10, 64, 18), new Color(246, 112, 122));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x, y + 26, 30, 14), new Color(186, 44, 70));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 118, y + 26, 30, 14), new Color(186, 44, 70));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 10, y + 40, 14, 20), new Color(246, 112, 122));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 124, y + 40, 14, 20), new Color(246, 112, 122));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 58, y + 16, 10, 8), new Color(255, 245, 225));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 82, y + 16, 10, 8), new Color(255, 245, 225));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 62, y + 20, 3, 3), new Color(25, 20, 30));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 86, y + 20, 3, 3), new Color(25, 20, 30));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 42, y + 60, 6, 18), new Color(255, 150, 70));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 66, y + 60, 6, 18), new Color(255, 150, 70));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 90, y + 60, 6, 18), new Color(255, 150, 70));
                PrimitiveRenderer.DrawOutline(spriteBatch, pixel, Bounds, 2, new Color(100, 20, 40));
                break;
            default:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 6, y + 8, 24, 12), new Color(96, 232, 120));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 10, y + 2, 16, 8), new Color(232, 255, 220));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 4, y + 18, 28, 6), new Color(36, 148, 72));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 14, y + 24, 8, 4), new Color(240, 188, 92));
                break;
        }
    }

    private void UpdateDiver(float deltaTime, Vector2 playerCenter)
    {
        if (!_hasTarget)
        {
            _targetX = playerCenter.X - (Size.X * 0.5f);
            _hasTarget = true;
        }

        var stepX = MathHelper.Clamp(_targetX - Position.X, -_horizontalSpeed * deltaTime, _horizontalSpeed * deltaTime);
        Position += new Vector2(stepX, _baseSpeed * 1.15f * deltaTime);
    }

    private void UpdateShooter(float deltaTime, List<Projectile> enemyProjectiles, Random random)
    {
        Position += new Vector2(MathF.Sin((_elapsedTime * 2.8f) + _spawnIndex) * _horizontalSpeed * deltaTime, _baseSpeed * 0.7f * deltaTime);

        _attackCooldown -= deltaTime;

        if (_attackCooldown > 0f)
        {
            return;
        }

        enemyProjectiles.Add(new Projectile(new Vector2(Center.X - 3f, Position.Y + Size.Y), new Vector2(0f, 300f + (_levelNumber * 16f)), ProjectileType.EnemyLaser));
        _attackCooldown = 1.35f - MathF.Min(0.18f, _levelNumber * 0.03f) + (random.NextSingle() * 0.45f);
    }

    private void UpdateMiner(float deltaTime, List<Projectile> enemyProjectiles, Random random)
    {
        Position += new Vector2(MathF.Sin((_elapsedTime * 5.6f) + _spawnIndex) * _horizontalSpeed * deltaTime, _baseSpeed * 1.08f * deltaTime);

        _secondaryCooldown -= deltaTime;

        if (_secondaryCooldown > 0f)
        {
            return;
        }

        var drift = MathF.Sin(_elapsedTime + _spawnIndex) * 28f;
        enemyProjectiles.Add(new Projectile(new Vector2(Center.X - 7f, Position.Y + Size.Y), new Vector2(drift, 100f + (_levelNumber * 12f)), ProjectileType.Mine));
        _secondaryCooldown = 1.4f + (random.NextSingle() * 0.7f);
    }

    private void UpdateBoss(float deltaTime, Vector2 playerCenter, Rectangle movementBounds, List<Projectile> enemyProjectiles, Random random)
    {
        var nextX = Position.X + (_bossDirection * _horizontalSpeed * deltaTime);

        if (nextX <= movementBounds.Left + 26f || nextX + Size.X >= movementBounds.Right - 26f)
        {
            _bossDirection *= -1f;
            nextX = MathHelper.Clamp(nextX, movementBounds.Left + 26f, movementBounds.Right - Size.X - 26f);
        }

        Position = new Vector2(nextX, 56f + (MathF.Sin(_elapsedTime * 1.5f) * 14f));

        _attackCooldown -= deltaTime;

        if (_attackCooldown <= 0f)
        {
            var aimX = Math.Clamp(playerCenter.X - Center.X, -140f, 140f);
            enemyProjectiles.Add(new Projectile(new Vector2(Center.X - 20f, Position.Y + Size.Y - 6f), new Vector2((aimX * 0.25f) - 110f, 250f + (_levelNumber * 16f)), ProjectileType.EnemyLaser));
            enemyProjectiles.Add(new Projectile(new Vector2(Center.X - 3f, Position.Y + Size.Y - 4f), new Vector2(aimX * 0.4f, 292f + (_levelNumber * 18f)), ProjectileType.EnemyLaser));
            enemyProjectiles.Add(new Projectile(new Vector2(Center.X + 14f, Position.Y + Size.Y - 6f), new Vector2((aimX * 0.25f) + 110f, 250f + (_levelNumber * 16f)), ProjectileType.EnemyLaser));
            _attackCooldown = 0.75f + (random.NextSingle() * 0.25f);
        }

        _secondaryCooldown -= deltaTime;

        if (_secondaryCooldown > 0f)
        {
            return;
        }

        enemyProjectiles.Add(new Projectile(new Vector2(Position.X + 26f, Position.Y + Size.Y - 12f), new Vector2(-40f, 132f), ProjectileType.Mine));
        enemyProjectiles.Add(new Projectile(new Vector2(Position.X + Size.X - 40f, Position.Y + Size.Y - 12f), new Vector2(40f, 132f), ProjectileType.Mine));
        _secondaryCooldown = 2.05f + (random.NextSingle() * 0.55f);
    }

    private static Vector2 ResolveSize(EnemyType type)
    {
        return type switch
        {
            EnemyType.Green => new Vector2(36f, 28f),
            EnemyType.Red => new Vector2(38f, 30f),
            EnemyType.Blue => new Vector2(42f, 34f),
            EnemyType.Yellow => new Vector2(40f, 30f),
            _ => new Vector2(148f, 92f)
        };
    }
}
