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

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, GameConfig.ScreenBounds, level.PrimaryTint * 0.12f);

        switch (level.Number)
        {
            case 1:
                DrawPixelDisc(new Vector2(194f, 704f), 58, level.LandmarkTint, 4);
                DrawPixelDisc(new Vector2(148f, 670f), 14, new Color(96, 208, 138), 4);
                DrawPixelDisc(new Vector2(228f, 730f), 11, new Color(92, 198, 126), 4);
                DrawPixelDisc(new Vector2(278f, 688f), 10, new Color(84, 188, 114), 4);
                DrawSpaceStation(new Vector2(560f, 138f), level.AccentTint);
                break;
            case 2:
                for (var y = 102; y < 582; y += 112)
                {
                    PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(30, y, GameConfig.ScreenWidth - 60, 2), new Color(112, 118, 170) * 0.18f);
                }

                DrawPixelDisc(new Vector2(480f, 244f), 44, level.LandmarkTint, 4);
                DrawPixelDisc(new Vector2(446f, 220f), 10, new Color(192, 198, 206), 4);
                DrawPixelDisc(new Vector2(508f, 266f), 8, new Color(188, 194, 204), 4);
                DrawPixelDisc(new Vector2(474f, 286f), 7, new Color(184, 190, 198), 4);
                break;
            case 3:
                DrawRingBand(new Vector2(472f, 282f), 254, 7, new Color(228, 216, 192) * 0.55f, 12);
                DrawPixelDisc(new Vector2(482f, 284f), 50, level.LandmarkTint, 4);
                DrawPixelDisc(new Vector2(492f, 220f), 8, new Color(206, 196, 172), 4);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(308, 298, 116, 10), new Color(242, 232, 214) * 0.82f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(556, 298, 126, 10), new Color(242, 232, 214) * 0.82f);
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(562, 286, 164, 58), new Color(18, 10, 24) * 0.42f);
                break;
            case 4:
                PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(524, 152, 240, 208), level.AccentTint * 0.06f);
                DrawPixelDisc(new Vector2(620f, 264f), 52, level.LandmarkTint, 4);
                DrawPixelDisc(new Vector2(596f, 238f), 10, new Color(138, 222, 255), 4);
                DrawPixelDisc(new Vector2(638f, 282f), 8, new Color(124, 206, 255), 4);
                DrawPixelDisc(new Vector2(654f, 220f), 7, new Color(124, 206, 255), 4);
                break;
            default:
                DrawPixelDisc(new Vector2(480f, 274f), 52, level.LandmarkTint, 4);
                DrawPixelDisc(new Vector2(450f, 246f), 10, new Color(198, 58, 38), 4);
                DrawPixelDisc(new Vector2(508f, 296f), 9, new Color(202, 62, 38), 4);
                DrawPixelDisc(new Vector2(520f, 232f), 7, new Color(255, 146, 86), 4);
                DrawDustBand(356, new Color(255, 226, 212) * 0.28f);
                break;
        }
    }

    private void DrawPixelDisc(Vector2 center, int radius, Color color, int pixelScale)
    {
        for (var y = -radius; y <= radius; y++)
        {
            var span = (int)MathF.Sqrt((radius * radius) - (y * y));
            var drawY = (int)center.Y + (y * pixelScale);
            var drawX = (int)center.X - (span * pixelScale);
            var drawWidth = ((span * 2) + 1) * pixelScale;

            PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(drawX, drawY, drawWidth, pixelScale), color);
        }
    }

    private void DrawRingBand(Vector2 center, int halfWidth, int strips, Color color, int taper)
    {
        for (var i = 0; i < strips; i++)
        {
            var distanceFromCenter = Math.Abs(i - (strips / 2));
            var inset = distanceFromCenter * taper;
            var y = (int)center.Y + ((i - (strips / 2)) * 6);
            var width = (halfWidth * 2) - (inset * 2);

            PrimitiveRenderer.DrawRect(
                _game.SpriteBatch,
                _game.Pixel,
                new Rectangle((int)center.X - halfWidth + inset, y, width, 4),
                color);
        }
    }

    private void DrawSpaceStation(Vector2 origin, Color accentColor)
    {
        var x = (int)origin.X;
        var y = (int)origin.Y;

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x, y, 180, 18), new Color(214, 228, 246));
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x + 68, y - 40, 16, 96), new Color(182, 204, 236));
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x + 146, y - 18, 28, 52), new Color(208, 220, 242));
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x - 70, y + 6, 70, 6), accentColor * 0.78f);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x + 180, y + 6, 78, 6), accentColor * 0.78f);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x - 24, y - 46, 6, 102), new Color(190, 206, 232));
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x - 50, y - 2, 28, 14), new Color(112, 150, 214));
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x + 196, y - 2, 28, 14), new Color(112, 150, 214));
    }

    private void DrawDustBand(int centerY, Color color)
    {
        for (var i = 0; i < 72; i++)
        {
            var x = 24 + ((i * 13) % (GameConfig.ScreenWidth - 48));
            var y = centerY + (((i * 29) % 28) - 14);
            var size = 2 + (i % 3);

            PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(x, y, size, size), color);
        }
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
