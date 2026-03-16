using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public enum PowerUpType
{
    ExtraLife,
    TripleShot,
    Bomb,
    Shield
}

public sealed class PowerUp : Entity
{
    private readonly Color _color;

    public PowerUp(PowerUpType type, Vector2 position)
        : base(position, new Vector2(26f, 26f))
    {
        Type = type;

        switch (type)
        {
            case PowerUpType.ExtraLife:
                DurationSeconds = 0f;
                _color = new Color(230, 70, 96);
                break;
            case PowerUpType.TripleShot:
                DurationSeconds = 10f;
                _color = new Color(116, 235, 255);
                break;
            case PowerUpType.Bomb:
                DurationSeconds = 0f;
                _color = new Color(255, 186, 86);
                break;
            default:
                DurationSeconds = 5f;
                _color = new Color(135, 255, 200);
                break;
        }
    }

    public PowerUpType Type { get; }
    public float DurationSeconds { get; }

    public override void Update(float deltaTime)
    {
        Position += new Vector2(0f, 120f * deltaTime);
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

        switch (Type)
        {
            case PowerUpType.ExtraLife:
                PrimitiveRenderer.DrawHeart(spriteBatch, pixel, new Vector2(rectangle.X + 4, rectangle.Y + 5), 2, new Color(255, 236, 242));
                break;
            case PowerUpType.TripleShot:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 5, rectangle.Y + 5, 3, 14), new Color(10, 55, 80));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 11, rectangle.Y + 3, 3, 18), new Color(10, 55, 80));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 17, rectangle.Y + 5, 3, 14), new Color(10, 55, 80));
                break;
            case PowerUpType.Bomb:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 8, rectangle.Y + 8, 10, 10), new Color(55, 32, 24));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 11, rectangle.Y + 3, 4, 5), new Color(255, 245, 165));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 4, rectangle.Y + 12, 4, 2), new Color(255, 245, 165));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 18, rectangle.Y + 12, 4, 2), new Color(255, 245, 165));
                break;
            default:
                PrimitiveRenderer.DrawOutline(spriteBatch, pixel, new Rectangle(rectangle.X + 5, rectangle.Y + 5, 16, 16), 2, new Color(235, 250, 255));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 11, rectangle.Y + 11, 4, 4), new Color(235, 250, 255));
                break;
        }
    }
}
