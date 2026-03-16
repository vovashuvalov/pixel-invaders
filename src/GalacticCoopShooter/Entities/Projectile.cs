using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public enum ProjectileType
{
    PlayerShot,
    EnemyLaser,
    Mine
}

public sealed class Projectile : Entity
{
    private readonly Color _primaryColor;
    private readonly Color _secondaryColor;
    private float _lifetimeRemaining;

    public Projectile(Vector2 position, Vector2 velocity, ProjectileType type)
        : base(position, ResolveSize(type))
    {
        Velocity = velocity;
        Type = type;
        Damage = 1;

        (_primaryColor, _secondaryColor, _lifetimeRemaining) = type switch
        {
            ProjectileType.PlayerShot => (new Color(120, 255, 185), new Color(220, 255, 235), float.MaxValue),
            ProjectileType.Mine => (new Color(255, 196, 74), new Color(255, 110, 74), 8f),
            _ => (new Color(255, 110, 110), new Color(255, 226, 130), float.MaxValue)
        };
    }

    public Vector2 Velocity { get; }
    public ProjectileType Type { get; }
    public int Damage { get; }

    public override void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        _lifetimeRemaining -= deltaTime;
    }

    public bool IsOutOfBounds(Rectangle bounds)
    {
        return _lifetimeRemaining <= 0f
            || Position.Y < -Size.Y - 32f
            || Position.Y > bounds.Bottom + Size.Y + 32f
            || Position.X < -Size.X - 32f
            || Position.X > bounds.Right + Size.X + 32f;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        var rectangle = Bounds;

        switch (Type)
        {
            case ProjectileType.PlayerShot:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, rectangle, _primaryColor);
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 1, rectangle.Y, 2, rectangle.Height), _secondaryColor);
                break;
            case ProjectileType.Mine:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, rectangle, _primaryColor);
                PrimitiveRenderer.DrawOutline(spriteBatch, pixel, rectangle, 1, new Color(95, 35, 20));
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X + 3, rectangle.Y + 3, rectangle.Width - 6, rectangle.Height - 6), _secondaryColor);
                break;
            default:
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, rectangle, _primaryColor);
                PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(rectangle.X - 1, rectangle.Y + rectangle.Height - 4, rectangle.Width + 2, 4), _secondaryColor);
                break;
        }
    }

    private static Vector2 ResolveSize(ProjectileType type)
    {
        return type switch
        {
            ProjectileType.PlayerShot => new Vector2(4f, 18f),
            ProjectileType.Mine => new Vector2(14f, 14f),
            _ => new Vector2(6f, 16f)
        };
    }
}
