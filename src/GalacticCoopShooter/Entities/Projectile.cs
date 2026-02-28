using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public sealed class Projectile : Entity
{
    private readonly Color _color;

    public Projectile(Vector2 position, Vector2 velocity, bool isEnemyProjectile, Color color)
        : base(position, new Vector2(6f, 16f))
    {
        Velocity = velocity;
        IsEnemyProjectile = isEnemyProjectile;
        _color = color;
    }

    public Vector2 Velocity { get; }
    public bool IsEnemyProjectile { get; }

    public override void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }

    public bool IsOutOfBounds(Rectangle bounds)
    {
        return Position.Y < -Size.Y || Position.Y > bounds.Bottom + Size.Y;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        var rectangle = Bounds;
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, rectangle, _color);

        if (!IsEnemyProjectile)
        {
            PrimitiveRenderer.DrawRect(
                spriteBatch,
                pixel,
                new Rectangle(rectangle.X - 1, rectangle.Y + 4, rectangle.Width + 2, rectangle.Height - 8),
                new Color(150, 255, 170));
            return;
        }

        PrimitiveRenderer.DrawRect(
            spriteBatch,
            pixel,
            new Rectangle(rectangle.X - 1, rectangle.Y + rectangle.Height - 4, rectangle.Width + 2, 4),
            new Color(250, 190, 100));
    }
}
