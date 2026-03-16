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
        DrawCenteredText("PIXEL INVADERS", 120, 5, new Color(230, 245, 255));
        DrawCenteredText("RETRO VERTICAL SHOOTER", 196, 2, new Color(120, 195, 255));
        DrawCenteredText("5 LEVELS  5 ALIEN TYPES  4 POWERUPS", 242, 2, new Color(170, 210, 255));

        for (var i = 0; i < _menuOptions.Length; i++)
        {
            var color = i == _selectedIndex ? new Color(120, 255, 175) : new Color(220, 225, 245);
            DrawCenteredText(_menuOptions[i], 324 + (i * 58), 3, color);
        }

        DrawCenteredText("UP DOWN OR W S  ENTER TO SELECT", 584, 2, new Color(165, 185, 215));

        if (!_showControls)
        {
            return;
        }

        PrimitiveRenderer.DrawRect(_game.SpriteBatch, _game.Pixel, new Rectangle(140, 170, 680, 360), new Color(0, 0, 0, 210));
        PrimitiveRenderer.DrawOutline(_game.SpriteBatch, _game.Pixel, new Rectangle(140, 170, 680, 360), 2, new Color(110, 165, 245));

        DrawCenteredText("CONTROLS", 208, 3, new Color(235, 245, 255));
        DrawCenteredText("MOVE  WASD OR ARROWS", 272, 2, new Color(205, 220, 245));
        DrawCenteredText("FIRE  HOLD SPACE OR LEFT MOUSE", 314, 2, new Color(205, 220, 245));
        DrawCenteredText("PAUSE  ESC", 356, 2, new Color(205, 220, 245));
        DrawCenteredText("POWERUPS  LIFE  TRIPLE  BOMB  SHIELD", 410, 2, new Color(205, 220, 245));
        DrawCenteredText("CLEAR 5 LEVELS AND DEFEAT EVERY BOSS", 452, 2, new Color(205, 220, 245));
        DrawCenteredText("PRESS ENTER TO RETURN", 494, 2, new Color(120, 255, 175));
    }

    private void DrawCenteredText(string text, int y, int scale, Color color)
    {
        var size = _game.Font.MeasureText(text, scale);
        var x = (GameConfig.ScreenWidth - size.X) * 0.5f;
        _game.Font.DrawText(_game.SpriteBatch, _game.Pixel, text, new Vector2(x, y), scale, color);
    }
}
