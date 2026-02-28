using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Core;

public interface IGameState
{
    void Enter();
    void Exit();
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime);
}
