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
    private readonly List<Projectile> _enemyEggs = [];
    private readonly List<PowerUp> _powerUps = [];

    private int _score;
    private float _waveBannerTimer;
    private float _nextWaveDelay;
    private bool _waitingForNextWave;

    public PlayState(ShooterGame game)
    {
        _game = game;
        _player = new Player(new Vector2((GameConfig.ScreenWidth * 0.5f) - 26f, GameConfig.ScreenHeight - 92f));
        _waveManager = new WaveManager(GameConfig.EnemyMovementBounds, game.Random);
        _collisionManager = new CollisionManager();
    }

    public int Score => _score;
    public int Wave => _waveManager.CurrentWave;

    public void Enter()
    {
        if (_waveManager.CurrentWave == 0)
        {
            _waveManager.StartWave(1);
            _waveBannerTimer = 1.5f;
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

        _waveManager.Update(deltaTime, _enemyEggs);

        UpdateProjectiles(deltaTime);
        UpdatePowerUps(deltaTime);

        var collisionReport = _collisionManager.Resolve(
            _player,
            _playerBullets,
            _enemyEggs,
            _waveManager.Enemies,
            _powerUps,
            _game.Random);

        _score += collisionReport.ScoreDelta;

        _waveManager.PruneInactive();

        _playerBullets.RemoveAll(bullet => !bullet.IsActive || bullet.IsOutOfBounds(GameConfig.ScreenBounds));
        _enemyEggs.RemoveAll(egg => !egg.IsActive || egg.IsOutOfBounds(GameConfig.ScreenBounds));
        _powerUps.RemoveAll(powerUp => !powerUp.IsActive || powerUp.IsOutOfBounds(GameConfig.ScreenBounds));

        if (_player.Lives <= 0)
        {
            _game.StateManager.ChangeState(new GameOverState(_game, _score, _waveManager.CurrentWave, false));
            return;
        }

        if (_waitingForNextWave)
        {
            _nextWaveDelay -= deltaTime;

            if (_nextWaveDelay <= 0f)
            {
                _waitingForNextWave = false;
                var nextWave = _waveManager.CurrentWave + 1;

                if (nextWave > GameConfig.TotalWaves)
                {
                    _game.StateManager.ChangeState(new GameOverState(_game, _score, _waveManager.CurrentWave, true));
                    return;
                }

                _waveManager.StartWave(nextWave);
                _waveBannerTimer = 1.4f;
            }
        }
        else if (_waveManager.IsWaveCleared)
        {
            if (_waveManager.CurrentWave >= GameConfig.TotalWaves)
            {
                _game.StateManager.ChangeState(new GameOverState(_game, _score, _waveManager.CurrentWave, true));
                return;
            }

            _waitingForNextWave = true;
            _nextWaveDelay = 1.15f;
        }

        if (_waveBannerTimer > 0f)
        {
            _waveBannerTimer -= deltaTime;
        }
    }

    public void Draw(GameTime gameTime)
    {
        for (var i = 0; i < _waveManager.Enemies.Count; i++)
        {
            _waveManager.Enemies[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _playerBullets.Count; i++)
        {
            _playerBullets[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _enemyEggs.Count; i++)
        {
            _enemyEggs[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        for (var i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].Draw(_game.SpriteBatch, _game.Pixel);
        }

        _player.Draw(_game.SpriteBatch, _game.Pixel);

        DrawHud();

        if (_waveBannerTimer > 0f)
        {
            DrawCenteredText($"WAVE {_waveManager.CurrentWave}", 300, 4, new Color(235, 245, 255));
        }

        if (_waitingForNextWave)
        {
            DrawCenteredText("WAVE CLEARED", 350, 2, new Color(120, 255, 170));
        }
    }

    private void DrawHud()
    {
        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(0, 0, GameConfig.ScreenWidth, 62), new Color(0, 0, 0, 160));

        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, $"SCORE:{_score}", new Vector2(20, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, $"WAVE:{_waveManager.CurrentWave}/{GameConfig.TotalWaves}", new Vector2(350, 18), 2, new Color(240, 245, 255));
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, "LIVES:", new Vector2(740, 18), 2, new Color(240, 245, 255));

        for (var i = 0; i < 3; i++)
        {
            var color = i < _player.Lives ? new Color(235, 55, 75) : new Color(70, 70, 80);
            PrimitiveRenderer.DrawHeart(_game.SpriteBatch, _game.Pixel, new Vector2(828 + (i * 34), 18), 3, color);
        }

        var indicatorY = 68;

        if (_player.IsRapidFireActive)
        {
            _game.Font.DrawText(
                _game.SpriteBatch,
                _game.Pixel,
                $"RAPID FIRE:{MathF.Ceiling(_player.RapidFireRemaining)}S",
                new Vector2(16, indicatorY),
                2,
                new Color(255, 195, 120));
            indicatorY += 22;
        }

        if (_player.IsDoubleShotActive)
        {
            _game.Font.DrawText(
                _game.SpriteBatch,
                _game.Pixel,
                $"DOUBLE SHOT:{MathF.Ceiling(_player.DoubleShotRemaining)}S",
                new Vector2(16, indicatorY),
                2,
                new Color(125, 245, 255));
        }
    }

    private void UpdateProjectiles(float deltaTime)
    {
        for (var i = 0; i < _playerBullets.Count; i++)
        {
            _playerBullets[i].Update(deltaTime);
        }

        for (var i = 0; i < _enemyEggs.Count; i++)
        {
            _enemyEggs[i].Update(deltaTime);
        }
    }

    private void UpdatePowerUps(float deltaTime)
    {
        for (var i = 0; i < _powerUps.Count; i++)
        {
            _powerUps[i].Update(deltaTime);
        }
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
