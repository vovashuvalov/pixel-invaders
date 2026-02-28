using GalacticCoopShooter.Core;
using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.States;

public sealed class PauseState : IGameState
{
    private readonly ShooterGame _game;
    private readonly PlayState _playState;
    private readonly string[] _options = ["RESUME", "RESTART", "MAIN MENU"];

    private int _selectedIndex;

    public PauseState(ShooterGame game, PlayState playState)
    {
        _game = game;
        _playState = playState;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (_game.Input.IsPressed(Keys.Escape))
        {
            _game.StateManager.ChangeState(_playState);
            return;
        }

        if (_game.Input.IsPressedAny(Keys.Up, Keys.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
        }

        if (_game.Input.IsPressedAny(Keys.Down, Keys.S))
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
                _game.StateManager.ChangeState(_playState);
                break;
            case 1:
                _game.StateManager.ChangeState(new PlayState(_game));
                break;
            case 2:
                _game.StateManager.ChangeState(new MenuState(_game));
                break;
        }
    }

    public void Draw(GameTime gameTime)
    {
        _playState.Draw(gameTime);

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, GameConfig.ScreenBounds, new Color(0, 0, 0, 180));
        PrimitiveRenderer.DrawOutline(_game.SpriteBatch, _game.Pixel, new Rectangle(240, 180, 480, 320), 2, new Color(130, 180, 245));

        DrawCenteredText("PAUSED", 220, 4, new Color(235, 245, 255));

        for (var i = 0; i < _options.Length; i++)
        {
            var color = i == _selectedIndex ? new Color(120, 255, 170) : new Color(220, 225, 245);
            DrawCenteredText(_options[i], 300 + (i * 60), 3, color);
        }

        DrawCenteredText("ESC TO RESUME", 470, 2, new Color(170, 190, 225));
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
