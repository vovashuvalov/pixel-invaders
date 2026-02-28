using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public abstract class Entity
{
    protected Entity(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    public Vector2 Position { get; protected set; }
    public Vector2 Size { get; protected set; }
    public bool IsActive { get; set; } = true;

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

    public virtual void Update(float deltaTime)
    {
    }

    public abstract void Draw(SpriteBatch spriteBatch, Texture2D pixel);
}
