using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Core;

public sealed class GameStateManager
{
    private IGameState? _currentState;

    public IGameState? CurrentState => _currentState;

    public void ChangeState(IGameState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update(GameTime gameTime)
    {
        _currentState?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _currentState?.Draw(gameTime);
    }
}
