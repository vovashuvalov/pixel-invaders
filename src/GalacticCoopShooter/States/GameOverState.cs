using GalacticCoopShooter.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.States;

public sealed class GameOverState : IGameState
{
    private readonly ShooterGame _game;
    private readonly int _finalScore;
    private readonly int _reachedWave;
    private readonly bool _won;

    private readonly string[] _options = ["RESTART", "MAIN MENU"];
    private int _selectedIndex;

    public GameOverState(ShooterGame game, int finalScore, int reachedWave, bool won)
    {
        _game = game;
        _finalScore = finalScore;
        _reachedWave = reachedWave;
        _won = won;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (_game.Input.IsPressedAny(Keys.Up, Keys.W, Keys.Down, Keys.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _options.Length;
        }

        if (!_game.Input.IsPressedAny(Keys.Enter, Keys.Space))
        {
            return;
        }

        switch (_selectedIndex)
        {
            case 0:
                _game.StateManager.ChangeState(new PlayState(_game));
                break;
            case 1:
                _game.StateManager.ChangeState(new MenuState(_game));
                break;
        }
    }

    public void Draw(GameTime gameTime)
    {
        var title = _won ? "VICTORY" : "GAME OVER";
        var subtitle = _won ? "YOU CLEARED ALL WAVES" : "THE CHICKENS OVERWHELMED YOU";

        DrawCenteredText(title, 180, 5, _won ? new Color(130, 255, 180) : new Color(255, 125, 120));
        DrawCenteredText(subtitle, 262, 2, new Color(210, 220, 240));
        DrawCenteredText($"FINAL SCORE:{_finalScore}", 318, 3, new Color(235, 245, 255));
        DrawCenteredText($"WAVE REACHED:{_reachedWave}", 366, 2, new Color(195, 210, 240));

        for (var i = 0; i < _options.Length; i++)
        {
            var color = i == _selectedIndex ? new Color(120, 255, 175) : new Color(225, 230, 245);
            DrawCenteredText(_options[i], 450 + (i * 58), 3, color);
        }
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
