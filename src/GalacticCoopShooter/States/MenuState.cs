using GalacticCoopShooter.Core;
using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.States;

public sealed class MenuState : IGameState
{
    private readonly ShooterGame _game;
    private readonly string[] _menuOptions = ["START", "CONTROLS", "QUIT"];

    private int _selectedIndex;
    private bool _showControls;

    public MenuState(ShooterGame game)
    {
        _game = game;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (_showControls)
        {
            if (_game.Input.IsPressedAny(Keys.Enter, Keys.Space, Keys.Escape))
            {
                _showControls = false;
            }

            return;
        }

        if (_game.Input.IsPressedAny(Keys.Up, Keys.W))
        {
            _selectedIndex = (_selectedIndex - 1 + _menuOptions.Length) % _menuOptions.Length;
        }

        if (_game.Input.IsPressedAny(Keys.Down, Keys.S))
        {
            _selectedIndex = (_selectedIndex + 1) % _menuOptions.Length;
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
                _showControls = true;
                break;
            case 2:
                _game.Exit();
                break;
        }
    }

    public void Draw(GameTime gameTime)
    {
        DrawCenteredText("GALACTIC COOP SHOOTER", 120, 4, new Color(230, 245, 255));
        DrawCenteredText("SPACE CHICKEN ARCADE", 190, 2, new Color(120, 195, 255));

        for (var i = 0; i < _menuOptions.Length; i++)
        {
            var color = i == _selectedIndex ? new Color(120, 255, 175) : new Color(220, 225, 245);
            DrawCenteredText(_menuOptions[i], 300 + (i * 58), 3, color);
        }

        DrawCenteredText("UP DOWN OR W S  ENTER TO SELECT", 560, 2, new Color(165, 185, 215));

        if (!_showControls)
        {
            return;
        }

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(140, 170, 680, 360), new Color(0, 0, 0, 210));
        PrimitiveRenderer.DrawOutline(_game.SpriteBatch, _game.Pixel, new Rectangle(140, 170, 680, 360), 2, new Color(110, 165, 245));

        DrawCenteredText("CONTROLS", 208, 3, new Color(235, 245, 255));
        DrawCenteredText("MOVE  A D OR LEFT RIGHT", 280, 2, new Color(205, 220, 245));
        DrawCenteredText("SHOOT SPACE", 324, 2, new Color(205, 220, 245));
        DrawCenteredText("PAUSE ESC", 368, 2, new Color(205, 220, 245));
        DrawCenteredText("DESTROY WAVES OF SPACE CHICKENS", 412, 2, new Color(205, 220, 245));
        DrawCenteredText("PRESS ENTER TO RETURN", 470, 2, new Color(120, 255, 175));
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
