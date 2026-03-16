using GalacticCoopShooter.Core;
using GalacticCoopShooter.Entities;
using GalacticCoopShooter.Gameplay;
using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.States;

public sealed class PlayState : IGameState
{
    private readonly ShooterGame _game;
    private readonly Player _player;
    private readonly WaveManager _waveManager;
    private readonly CollisionManager _collisionManager;

    private readonly List<Projectile> _playerBullets = [];
    private readonly List<Projectile> _enemyProjectiles = [];
    private readonly List<PowerUp> _powerUps = [];

    private int _score;
    private float _bannerTimer;
    private float _nextWaveDelay;
    private bool _waitingForNextWave;
    private string _bannerTitle = string.Empty;
    private string _bannerSubtitle = string.Empty;

    public PlayState(ShooterGame game)
    {
        _game = game;
        _player = new Player(new Vector2((GameConfig.ScreenWidth * 0.5f) - 26f, GameConfig.ScreenHeight - 92f));
        _waveManager = new WaveManager(GameConfig.EnemyMovementBounds, game.Random);
        _collisionManager = new CollisionManager();
    }

    public int Score => _score;
    public int Wave => _waveManager.CurrentWaveNumber;

    public void Enter()
    {
        if (_waveManager.CurrentWaveNumber == 0)
        {
            _waveManager.StartCampaign();
            SetEncounterBanner();
        }
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_game.Input.IsPressed(Keys.Escape))
        {
            _game.StateManager.ChangeState(new PauseState(_game, this));
            return;
        }

        _player.Tick(deltaTime, _game.Input, GameConfig.PlayerMovementBounds, _playerBullets);

        _waveManager.Update(deltaTime, _player.Center, _enemyProjectiles);

        UpdateProjectiles(deltaTime);
        UpdatePowerUps(deltaTime);
        ResolveEnemyBreaches();

        var collisionReport = _collisionManager.Resolve(
            _player,
            _playerBullets,
            _enemyProjectiles,
            _waveManager.Enemies,
            _powerUps,
            _game.Random);

        _score += collisionReport.ScoreDelta;
        ApplyCollectedPowerUps(collisionReport.CollectedPowerUps);

        _waveManager.PruneInactive();

        _playerBullets.RemoveAll(bullet => !bullet.IsActive || bullet.IsOutOfBounds(GameConfig.ScreenBounds));
        _enemyProjectiles.RemoveAll(projectile => !projectile.IsActive || projectile.IsOutOfBounds(GameConfig.ScreenBounds));
        _powerUps.RemoveAll(powerUp => !powerUp.IsActive || powerUp.IsOutOfBounds(GameConfig.ScreenBounds));

        if (_player.Lives <= 0)
        {
            _game.StateManager.ChangeState(new GameOverState(_game, _score, _waveManager.CurrentLevelNumber, _waveManager.CurrentLevel.Name, false));
            return;
        }

        if (_waitingForNextWave)
        {
            _nextWaveDelay -= deltaTime;

            if (_nextWaveDelay <= 0f)
            {
                _waitingForNextWave = false;
                if (!_waveManager.AdvanceEncounter())
                {
                    _game.StateManager.ChangeState(new GameOverState(_game, _score, GameConfig.TotalLevels, CampaignData.Levels[^1].Name, true));
                    return;
                }

                SetEncounterBanner();
            }
        }
        else if (_waveManager.IsWaveCleared)
        {
            _waitingForNextWave = true;
            _nextWaveDelay = 1.2f;
        }

        if (_bannerTimer > 0f)
        {
            _bannerTimer -= deltaTime;
        }
    }

    public void Draw(GameTime gameTime)
    {
        DrawLevelBackdrop();

        for (var i = 0; i < _waveManager.Enemies.Count; i++)
        {
            _waveManager.Enemies[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _playerBullets.Count; i++)
        {
            _playerBullets[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _enemyProjectiles.Count; i++)
        {
            _enemyProjectiles[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        _player.Draw(_game.SpriteBatch, _game.Pixel);

        DrawHud();

        if (_bannerTimer > 0f)
        {
            DrawCenteredText(_bannerTitle, 258, 4, new Color(235, 245, 255));
            DrawCenteredText(_bannerSubtitle, 326, 2, new Color(120, 230, 255));
        }

        if (_waitingForNextWave)
        {
            DrawCenteredText("SECTOR CLEAR", 384, 2, new Color(120, 255, 170));
        }
    }

    private void DrawHud()
    {
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(0, 0, GameConfig.ScreenWidth, 72), new Color(0, 0, 0, 168));

        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, $"SCORE:{_score}", new Vector2(20, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, $"LEVEL:{_waveManager.CurrentLevelNumber}", new Vector2(240, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, _waveManager.IsBossEncounter ? "WAVE:BOSS" : $"WAVE:{_waveManager.CurrentWaveNumber}/{GameConfig.RegularWavesPerLevel}", new Vector2(412, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, "LIVES:", new Vector2(730, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, _waveManager.CurrentLevel.Name, new Vector2(20, 46), 1, _waveManager.CurrentLevel.AccentTint);

        for (var i = 0; i < GameConfig.MaxLives; i++)
        {
            var color = i < _player.Lives ? new Color(235, 55, 75) : new Color(70, 70, 80);
            PrimitiveRenderer.DrawHeart(_game.SpriteBatch, _game.Pixel, new Vector2(810 + (i * 28), 18), 2, color);
        }

        var indicatorY = 82;

        if (_player.IsTripleShotActive)
        {
            _game.Font.DrawText(
                _game.SpriteBatch,
                _game.Pixel,
                $"TRIPLE SHOT:{MathF.Ceiling(_player.TripleShotRemaining)}S",
                new Vector2(16, indicatorY),
                2,
                new Color(125, 245, 255));
            indicatorY += 22;
        }

        if (_player.IsShieldActive)
        {
            _game.Font.DrawText(
                _game.SpriteBatch,
                _game.Pixel,
                $"SHIELD:{MathF.Ceiling(_player.ShieldRemaining)}S",
                new Vector2(16, indicatorY),
                2,
                new Color(140, 255, 195));
        }
    }

    private void UpdateProjectiles(float deltaTime)
    {
        for (var i = 0; i < _playerBullets.Count; i++)
        {
            _playerBullets[i].Update(deltaTime);
        }

        for (var i = 0; i < _enemyProjectiles.Count; i++)
        {
            _enemyProjectiles[i].Update(deltaTime);
        }
    }

    private void UpdatePowerUps(float deltaTime)
    {
        for (var i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].Update(deltaTime);
        }
    }

    private void ResolveEnemyBreaches()
    {
        for (var i = 0; i < _waveManager.Enemies.Count; i++)
        {
            var enemy = _waveManager.Enemies[i];

            if (!enemy.IsActive || !enemy.HasReachedEarth(GameConfig.ScreenBounds))
            {
                continue;
            }

            enemy.IsActive = false;
            _player.TryTakeDamage();
        }
    }

    private void ApplyCollectedPowerUps(IReadOnlyList<PowerUpType> collectedPowerUps)
    {
        for (var i = 0; i < collectedPowerUps.Count; i++)
        {
            var powerUpType = collectedPowerUps[i];

            switch (powerUpType)
            {
                case PowerUpType.Bomb:
                    DetonateBomb();
                    break;
                case PowerUpType.ExtraLife:
                    _player.ApplyPowerUp(powerUpType, 0f);
                    break;
                case PowerUpType.TripleShot:
                    _player.ApplyPowerUp(powerUpType, 10f);
                    break;
                case PowerUpType.Shield:
                    _player.ApplyPowerUp(powerUpType, 5f);
                    break;
            }
        }
    }

    private void DetonateBomb()
    {
        for (var i = 0; i < _waveManager.Enemies.Count; i++)
        {
            var enemy = _waveManager.Enemies[i];

            if (!enemy.IsActive)
            {
                continue;
            }

            if (enemy.IsBoss)
            {
                if (enemy.TakeDamage(8))
                {
                    _score += enemy.ScoreValue;
                }

                continue;
            }

            enemy.IsActive = false;
            _score += enemy.ScoreValue;
        }

        _enemyProjectiles.Clear();
    }

    private void SetEncounterBanner()
    {
        _bannerTitle = $"LEVEL {_waveManager.CurrentLevelNumber} {_waveManager.CurrentLevel.Name}";
        _bannerSubtitle = _waveManager.IsBossEncounter
            ? "BOSS WARNING"
            : $"WAVE {_waveManager.CurrentWaveNumber}";
        _bannerTimer = 1.8f;
    }

    private void DrawLevelBackdrop()
    {
        var level = _waveManager.CurrentLevel;

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, GameConfig.ScreenBounds, level.PrimaryTint * 0.10f);

        switch (level.Number)
        {
            case 1:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(48, 514, 180, 180), level.LandmarkTint * 0.85f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(92, 558, 92, 54), new Color(70, 210, 118) * 0.7f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(742, 110, 80, 80), level.AccentTint * 0.25f);
                break;
            case 2:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(662, 74, 168, 168), level.LandmarkTint * 0.85f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(706, 118, 28, 28), new Color(150, 150, 160));
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(756, 168, 20, 20), new Color(140, 140, 150));
                break;
            case 3:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(118, 128, 74, 44), level.LandmarkTint * 0.75f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(690, 218, 56, 34), level.AccentTint * 0.75f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(514, 476, 88, 56), level.LandmarkTint * 0.65f);
                break;
            case 4:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(708, 436, 192, 192), level.LandmarkTint * 0.82f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(88, 96, 96, 96), level.AccentTint * 0.22f);
                break;
            default:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(330, 86, 300, 300), level.AccentTint * 0.18f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(406, 162, 148, 148), level.LandmarkTint * 0.22f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(452, 208, 56, 56), new Color(255, 210, 180) * 0.28f);
                break;
        }
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
