using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public enum PowerUpType
{
    RapidFire,
    DoubleShot
}

public sealed class PowerUp : Entity
{
    private readonly Color _color;

    public PowerUp(PowerUpType type, Vector2 position)
        : base(position, new Vector2(22f, 22f))
    {
        Type = type;

        switch (type)
        {
            case PowerUpType.DoubleShot:
                DurationSeconds = 9f;
                _color = new Color(110, 240, 255);
                break;
            default:
                DurationSeconds = 7f;
                _color = new Color(255, 180, 82);
                break;
        }
    }

    public PowerUpType Type { get; }
    public float DurationSeconds { get; }

    public override void Update(float deltaTime)
    {
        Position += new Vector2(0f, 130f * deltaTime);
    }

    public bool IsOutOfBounds(Rectangle bounds)
    {
        return Position.Y > bounds.Bottom + Size.Y;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        var rectangle = Bounds;

        PrimitiveRenderer.DrawRect(spriteBatch, pixel, rectangle, _color * 0.95f);
        PrimitiveRenderer.DrawOutline(spriteBatch, pixel, rectangle, 1, new Color(245, 245, 245));

        if (Type == PowerUpType.RapidFire)
        {
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 8, rectangle.Y + 4, 3, 14), new Color(55, 30, 10));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 11, rectangle.Y + 4, 4, 4), new Color(55, 30, 10));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 6, rectangle.Y + 12, 5, 4), new Color(55, 30, 10));
        }
        else
        {
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 5, rectangle.Y + 4, 4, 14), new Color(10, 45, 75));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 13, rectangle.Y + 4, 4, 14), new Color(10, 45, 75));
        }
    }
}
