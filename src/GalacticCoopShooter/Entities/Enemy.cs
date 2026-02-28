using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalacticCoopShooter.Entities;

public enum EnemyType
{
    Normal,
    Tough,
    Rapid
}

public sealed class Enemy : Entity
{
    public Enemy(Vector2 position, EnemyType type, int gridColumn)
        : base(position, new Vector2(44f, 34f))
    {
        Type = type;
        GridColumn = gridColumn;

        switch (type)
        {
            case EnemyType.Tough:
                Health = 2;
                ScoreValue = 220;
                FireRateMultiplier = 0.9f;
                break;
            case EnemyType.Rapid:
                Health = 1;
                ScoreValue = 180;
                FireRateMultiplier = 1.7f;
                break;
            default:
                Health = 1;
                ScoreValue = 120;
                FireRateMultiplier = 1.0f;
                break;
        }
    }

    public EnemyType Type { get; }
    public int GridColumn { get; }
    public int Health { get; private set; }
    public int ScoreValue { get; }
    public float FireRateMultiplier { get; }
    public float ShotCooldown { get; set; }

    public void Move(Vector2 delta)
    {
        Position += delta;
    }

    public bool TakeDamage(int amount)
    {
        Health -= amount;

        if (Health > 0)
        {
            return false;
        }

        IsActive = false;
        return true;
    }

    public void ResetShotCooldown(Random random)
    {
        ShotCooldown = 0.55f + random.NextSingle() * 1.25f;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        var x = (int)Position.X;
        var y = (int)Position.Y;

        var bodyColor = Type switch
        {
            EnemyType.Tough => new Color(255, 196, 88),
            EnemyType.Rapid => new Color(150, 230, 255),
            _ => new Color(185, 215, 255)
        };

        var wingColor = Type == EnemyType.Rapid ? new Color(230, 245, 255) : new Color(245, 245, 250);

        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 8, y + 9, 28, 18), bodyColor);
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x, y + 14, 10, 10), wingColor);
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 34, y + 14, 10, 10), wingColor);
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 17, y + 3, 10, 8), new Color(255, 115, 90));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 20, y + 24, 4, 7), new Color(245, 180, 80));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 14, y + 14, 2, 2), new Color(24, 26, 40));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 28, y + 14, 2, 2), new Color(24, 26, 40));

        if (Type == EnemyType.Tough)
        {
            PrimitiveRenderer.DrawOutline(spriteBatch, pixel, new Rectangle(x + 7, y + 8, 30, 20), 1, new Color(120, 80, 10));
        }

        if (Type == EnemyType.Rapid)
        {
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 5, y + 11, 4, 4), new Color(110, 235, 255));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 35, y + 11, 4, 4), new Color(110, 235, 255));
        }
    }
}
