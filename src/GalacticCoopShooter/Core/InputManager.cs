using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.Core;

public sealed class InputManager
{
    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;

    public void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();
    }

    public bool IsDown(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key);
    }

    public bool IsDownAny(params Keys[] keys)
    {
        for (var i = 0; i < keys.Length; i++)
        {
            if (_currentKeyboard.IsKeyDown(keys[i]))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPressed(Keys key)
    {
        return _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    public bool IsPressedAny(params Keys[] keys)
    {
        for (var i = 0; i < keys.Length; i++)
        {
            if (_currentKeyboard.IsKeyDown(keys[i]) && !_previousKeyboard.IsKeyDown(keys[i]))
            {
                return true;
            }
        }

        return false;
    }
}
