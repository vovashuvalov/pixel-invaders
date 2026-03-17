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

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, GameConfig.ScreenBounds, level.PrimaryTint * 0.14f);

        switch (level.Number)
        {
            case 1:
                DrawMoonScene(level);
                break;
            case 2:
                DrawEarthStationScene(level);
                break;
            case 3:
                DrawRingWorldScene(level);
                break;
            case 4:
                DrawBluePlanetScene(level);
                break;
            default:
                DrawRedPlanetScene(level);
                break;
        }
    }

    private void DrawMoonScene(LevelDefinition level)
    {
        for (var y = 132; y <= 560; y += 108)
        {
            PrimitiveRenderer.DrawRect(
                _game.SpriteBatch,
                _game.Pixel,
                new Rectangle(18, y, GameConfig.ScreenWidth - 36, 3),
                new Color(82, 84, 130) * 0.16f);
        }

        DrawPixelDisc(new Vector2(480f, 208f), 38, new Color(236, 238, 236), 2);
        DrawPixelDisc(new Vector2(468f, 192f), 10, new Color(212, 218, 224), 2);
        DrawPixelDisc(new Vector2(498f, 212f), 12, new Color(210, 216, 222), 2);
        DrawPixelDisc(new Vector2(452f, 226f), 8, new Color(198, 204, 212), 2);
        DrawPixelDisc(new Vector2(490f, 238f), 7, new Color(202, 208, 216), 2);
        DrawPixelDisc(new Vector2(484f, 208f), 40, new Color(255, 255, 255) * 0.08f, 2);
    }

    private void DrawEarthStationScene(LevelDefinition level)
    {
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(88, 112, 172, 52), new Color(12, 18, 36) * 0.26f);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(612, 78, 204, 44), new Color(10, 14, 32) * 0.22f);

        DrawPixelDisc(new Vector2(250f, 696f), 92, level.LandmarkTint, 2);
        DrawPixelDisc(new Vector2(220f, 662f), 18, new Color(104, 206, 152), 2);
        DrawPixelDisc(new Vector2(274f, 644f), 16, new Color(78, 176, 118), 2);
        DrawPixelDisc(new Vector2(326f, 682f), 12, new Color(88, 188, 126), 2);
        DrawPixelDisc(new Vector2(206f, 718f), 12, new Color(220, 244, 250) * 0.45f, 2);

        DrawRotatedBar(new Vector2(312f, 184f), new Vector2(404f, 18f), -0.14f, new Color(216, 226, 246));
        DrawRotatedBar(new Vector2(390f, 134f), new Vector2(24f, 214f), 0.08f, new Color(186, 202, 230));
        DrawRotatedBar(new Vector2(644f, 154f), new Vector2(54f, 104f), -0.12f, new Color(206, 218, 238));
        DrawRotatedBar(new Vector2(298f, 210f), new Vector2(156f, 6f), -0.14f, new Color(112, 136, 174));
        DrawRotatedBar(new Vector2(498f, 194f), new Vector2(120f, 6f), -0.14f, new Color(112, 136, 174));
        DrawRotatedBar(new Vector2(360f, 188f), new Vector2(8f, 232f), -1.42f, new Color(194, 206, 230) * 0.75f);
        DrawRotatedBar(new Vector2(748f, 214f), new Vector2(84f, 4f), -1.44f, new Color(255, 255, 255) * 0.72f);

        DrawPixelDisc(new Vector2(614f, 126f), 4, new Color(235, 238, 246), 2);
        DrawFlare(new Vector2(690f, 284f), 18, new Color(255, 255, 255) * 0.82f);
    }

    private void DrawRingWorldScene(LevelDefinition level)
    {
        DrawRingBand(new Vector2(484f, 278f), 310, 9, new Color(232, 224, 208) * 0.56f, 10);
        DrawPixelDisc(new Vector2(484f, 270f), 52, new Color(218, 200, 168), 2);
        DrawPixelDisc(new Vector2(484f, 226f), 18, new Color(210, 196, 170), 2);
        DrawPixelDisc(new Vector2(492f, 202f), 8, new Color(88, 196, 226), 2);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(328, 286, 136, 12), new Color(244, 236, 220) * 0.86f);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(536, 286, 190, 12), new Color(244, 236, 220) * 0.86f);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(590, 240, 190, 98), new Color(16, 8, 22) * 0.46f);
        DrawFlare(new Vector2(492f, 274f), 9, new Color(255, 242, 184) * 0.65f);
    }

    private void DrawBluePlanetScene(LevelDefinition level)
    {
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(0, 356, GameConfig.ScreenWidth, 250), new Color(22, 64, 122) * 0.08f);
        DrawPixelDisc(new Vector2(484f, 246f), 54, level.LandmarkTint, 2);
        DrawRotatedBar(new Vector2(430f, 226f), new Vector2(104f, 12f), 0.16f, new Color(142, 154, 255) * 0.30f);
        DrawRotatedBar(new Vector2(444f, 270f), new Vector2(92f, 12f), -0.20f, new Color(150, 170, 255) * 0.26f);
        DrawPixelDisc(new Vector2(452f, 246f), 7, new Color(116, 234, 255), 2);
        DrawPixelDisc(new Vector2(506f, 266f), 8, new Color(106, 214, 255), 2);
        DrawPixelDisc(new Vector2(478f, 226f), 6, new Color(116, 228, 255), 2);
    }

    private void DrawRedPlanetScene(LevelDefinition level)
    {
        DrawDustBand(358, new Color(244, 232, 228) * 0.30f);
        DrawPixelDisc(new Vector2(482f, 252f), 52, level.LandmarkTint, 2);
        DrawPixelDisc(new Vector2(454f, 226f), 10, new Color(184, 40, 24), 2);
        DrawPixelDisc(new Vector2(500f, 204f), 8, new Color(255, 132, 76), 2);
        DrawPixelDisc(new Vector2(528f, 248f), 9, new Color(222, 58, 36), 2);
        DrawPixelDisc(new Vector2(470f, 284f), 8, new Color(204, 52, 32), 2);
        DrawPixelDisc(new Vector2(516f, 290f), 7, new Color(188, 44, 28), 2);
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

    private void DrawRotatedBar(Vector2 position, Vector2 size, float rotation, Color color)
    {
        _game.SpriteBatch.Draw(
            _game.Pixel,
            new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
            null,
            color,
            rotation,
            new Vector2(0f, size.Y * 0.5f),
            Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
            0f);
    }

    private void DrawFlare(Vector2 center, int radius, Color color)
    {
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle((int)center.X - radius, (int)center.Y - 1, radius * 2, 2), color);
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle((int)center.X - 1, (int)center.Y - radius, 2, radius * 2), color);
        DrawRotatedBar(new Vector2(center.X - radius, center.Y), new Vector2(radius * 2, 2), 0.78f, color * 0.82f);
        DrawRotatedBar(new Vector2(center.X - radius, center.Y), new Vector2(radius * 2, 2), -0.78f, color * 0.82f);
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
