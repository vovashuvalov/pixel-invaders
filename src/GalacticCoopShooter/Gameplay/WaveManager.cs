using GalacticCoopShooter.Core;
using GalacticCoopShooter.Entities;
using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public sealed class WaveManager
{
    private readonly Rectangle _movementBounds;
    private readonly Random _random;
    private readonly List<Enemy> _enemies = [];

    private int _levelIndex;
    private int _waveNumber;
    private bool _isBossEncounter;

    public WaveManager(Rectangle movementBounds, Random random)
    {
        _movementBounds = movementBounds;
        _random = random;
    }

    public IReadOnlyList<Enemy> Enemies => _enemies;
    public bool IsWaveCleared => _enemies.Count == 0;
    public bool IsBossEncounter => _isBossEncounter;
    public bool IsCampaignComplete { get; private set; }
    public LevelDefinition CurrentLevel => CampaignData.Levels[_levelIndex];
    public int CurrentLevelNumber => CurrentLevel.Number;
    public int CurrentWaveNumber => _waveNumber;

    public void StartCampaign()
    {
        _levelIndex = 0;
        _waveNumber = 1;
        _isBossEncounter = false;
        IsCampaignComplete = false;
        SpawnCurrentEncounter();
    }

    public bool AdvanceEncounter()
    {
        if (IsCampaignComplete)
        {
            return false;
        }

        if (_isBossEncounter)
        {
            if (_levelIndex >= CampaignData.Levels.Count - 1)
            {
                IsCampaignComplete = true;
                return false;
            }

            _levelIndex++;
            _waveNumber = 1;
            _isBossEncounter = false;
            SpawnCurrentEncounter();
            return true;
        }

        if (_waveNumber < GameConfig.RegularWavesPerLevel)
        {
            _waveNumber++;
            SpawnCurrentEncounter();
            return true;
        }

        _isBossEncounter = true;
        SpawnCurrentEncounter();
        return true;
    }

    public void Update(float deltaTime, Vector2 playerCenter, List<Projectile> enemyProjectiles)
    {
        for (var i = 0; i < _enemies.Count; i++)
        {
            _enemies[i].UpdateBehavior(deltaTime, playerCenter, _movementBounds, enemyProjectiles, _random);
        }
    }

    public void PruneInactive()
    {
        _enemies.RemoveAll(enemy => !enemy.IsActive);
    }

    private void SpawnCurrentEncounter()
    {
        _enemies.Clear();

        if (_isBossEncounter)
        {
            SpawnBoss();
            return;
        }

        var rows = GetWaveRows(CurrentLevelNumber, _waveNumber);

        for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            SpawnRow(rows[rowIndex], rowIndex);
        }
    }

    private void SpawnBoss()
    {
        var position = new Vector2((GameConfig.ScreenWidth * 0.5f) - 74f, 54f);
        _enemies.Add(new Enemy(position, EnemyType.Boss, CurrentLevelNumber, _waveNumber, 0));
    }

    private void SpawnRow(EnemyType[] row, int rowIndex)
    {
        var spacing = row.Length >= 6 ? 110f : 126f;
        var totalWidth = (row.Length - 1) * spacing;
        var startX = (GameConfig.ScreenWidth - totalWidth) * 0.5f - 18f;
        var y = -42f - (rowIndex * 82f);

        for (var i = 0; i < row.Length; i++)
        {
            var position = new Vector2(startX + (i * spacing), y - (_random.NextSingle() * 8f));
            _enemies.Add(new Enemy(position, row[i], CurrentLevelNumber, _waveNumber, rowIndex * 10 + i));
        }
    }

    private static EnemyType[][] GetWaveRows(int levelNumber, int waveNumber)
    {
        return levelNumber switch
        {
            1 => waveNumber switch
            {
                1 => [new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                2 => [new[] { EnemyType.Green, EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Green }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                3 => [new[] { EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                4 => [new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green }, new[] { EnemyType.Red, EnemyType.Green, EnemyType.Red }],
                5 => [new[] { EnemyType.Red, EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                _ => [new[] { EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }]
            },
            2 => waveNumber switch
            {
                1 => [new[] { EnemyType.Green, EnemyType.Blue, EnemyType.Green, EnemyType.Blue, EnemyType.Green }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Red }],
                2 => [new[] { EnemyType.Blue, EnemyType.Green, EnemyType.Blue, EnemyType.Green, EnemyType.Blue }, new[] { EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green }],
                3 => [new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Red, EnemyType.Blue, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                4 => [new[] { EnemyType.Blue, EnemyType.Blue, EnemyType.Green, EnemyType.Blue, EnemyType.Blue }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Red, EnemyType.Red }],
                5 => [new[] { EnemyType.Green, EnemyType.Blue, EnemyType.Red, EnemyType.Blue, EnemyType.Green }, new[] { EnemyType.Blue, EnemyType.Green, EnemyType.Blue }],
                _ => [new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Green, EnemyType.Blue, EnemyType.Red, EnemyType.Blue }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }]
            },
            3 => waveNumber switch
            {
                1 => [new[] { EnemyType.Yellow, EnemyType.Green, EnemyType.Blue, EnemyType.Green, EnemyType.Yellow }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Red }],
                2 => [new[] { EnemyType.Green, EnemyType.Yellow, EnemyType.Green, EnemyType.Yellow, EnemyType.Green }, new[] { EnemyType.Blue, EnemyType.Blue, EnemyType.Blue }],
                3 => [new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                4 => [new[] { EnemyType.Yellow, EnemyType.Red, EnemyType.Yellow, EnemyType.Red, EnemyType.Yellow }, new[] { EnemyType.Blue, EnemyType.Green, EnemyType.Blue }],
                5 => [new[] { EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Green, EnemyType.Red }],
                _ => [new[] { EnemyType.Yellow, EnemyType.Blue, EnemyType.Red, EnemyType.Blue, EnemyType.Yellow, EnemyType.Green }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }]
            },
            4 => waveNumber switch
            {
                1 => [new[] { EnemyType.Red, EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                2 => [new[] { EnemyType.Blue, EnemyType.Red, EnemyType.Yellow, EnemyType.Red, EnemyType.Blue }, new[] { EnemyType.Yellow, EnemyType.Green, EnemyType.Yellow }],
                3 => [new[] { EnemyType.Yellow, EnemyType.Blue, EnemyType.Red, EnemyType.Blue, EnemyType.Yellow }, new[] { EnemyType.Red, EnemyType.Green, EnemyType.Red, EnemyType.Green }],
                4 => [new[] { EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Red, EnemyType.Red }],
                5 => [new[] { EnemyType.Yellow, EnemyType.Red, EnemyType.Blue, EnemyType.Red, EnemyType.Yellow, EnemyType.Blue }, new[] { EnemyType.Green, EnemyType.Green, EnemyType.Green }],
                _ => [new[] { EnemyType.Red, EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue, EnemyType.Red, EnemyType.Yellow }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }]
            },
            _ => waveNumber switch
            {
                1 => [new[] { EnemyType.Blue, EnemyType.Yellow, EnemyType.Red, EnemyType.Yellow, EnemyType.Blue }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }],
                2 => [new[] { EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow }, new[] { EnemyType.Red, EnemyType.Red, EnemyType.Blue, EnemyType.Red }],
                3 => [new[] { EnemyType.Red, EnemyType.Yellow, EnemyType.Blue, EnemyType.Yellow, EnemyType.Red, EnemyType.Blue }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Green, EnemyType.Red }],
                4 => [new[] { EnemyType.Blue, EnemyType.Red, EnemyType.Yellow, EnemyType.Red, EnemyType.Blue, EnemyType.Yellow }, new[] { EnemyType.Yellow, EnemyType.Green, EnemyType.Yellow }],
                5 => [new[] { EnemyType.Yellow, EnemyType.Blue, EnemyType.Red, EnemyType.Blue, EnemyType.Yellow, EnemyType.Red }, new[] { EnemyType.Green, EnemyType.Red, EnemyType.Blue, EnemyType.Red }],
                _ => [new[] { EnemyType.Blue, EnemyType.Yellow, EnemyType.Red, EnemyType.Yellow, EnemyType.Blue, EnemyType.Red }, new[] { EnemyType.Yellow, EnemyType.Green, EnemyType.Yellow, EnemyType.Green }]
            }
        };
    }
}
