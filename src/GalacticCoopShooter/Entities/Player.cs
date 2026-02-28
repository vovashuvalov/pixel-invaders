using GalacticCoopShooter.Core;
using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.Entities;

public sealed class Player : Entity
{
    private const float MoveSpeed = 340f;
    private const float BaseFireCooldown = 0.32f;
    private const float RapidFireCooldown = 0.11f;

    private float _fireCooldownRemaining;
    private float _enginePulse;

    public Player(Vector2 position)
        : base(position, new Vector2(52f, 30f))
    {
    }

    public int Lives { get; private set; } = 3;
    public float RapidFireRemaining { get; private set; }
    public float DoubleShotRemaining { get; private set; }
    public float InvulnerabilityRemaining { get; private set; }

    public bool IsRapidFireActive => RapidFireRemaining > 0f;
    public bool IsDoubleShotActive => DoubleShotRemaining > 0f;

    public void Tick(float deltaTime, InputManager input, Rectangle movementBounds, List<Projectile> bullets)
    {
        if (!IsActive)
        {
            return;
        }

        _enginePulse += deltaTime;

        RapidFireRemaining = MathF.Max(0f, RapidFireRemaining - deltaTime);
        DoubleShotRemaining = MathF.Max(0f, DoubleShotRemaining - deltaTime);
        InvulnerabilityRemaining = MathF.Max(0f, InvulnerabilityRemaining - deltaTime);
        _fireCooldownRemaining = MathF.Max(0f, _fireCooldownRemaining - deltaTime);

        var direction = 0f;

        if (input.IsDownAny(Keys.A, Keys.Left))
        {
            direction -= 1f;
        }

        if (input.IsDownAny(Keys.D, Keys.Right))
        {
            direction += 1f;
        }

        Position += new Vector2(direction * MoveSpeed * deltaTime, 0f);

        var clampedX = MathHelper.Clamp(Position.X, movementBounds.Left, movementBounds.Right - Size.X);
        Position = new Vector2(clampedX, Position.Y);

        if (input.IsDown(Keys.Space) && _fireCooldownRemaining <= 0f)
        {
            Fire(bullets);
            _fireCooldownRemaining = IsRapidFireActive ? RapidFireCooldown : BaseFireCooldown;
        }
    }

    public bool TryTakeDamage()
    {
        if (InvulnerabilityRemaining > 0f)
        {
            return false;
        }

        Lives = Math.Max(0, Lives - 1);
        InvulnerabilityRemaining = 1.2f;
        return true;
    }

    public void ApplyPowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.DoubleShot:
                DoubleShotRemaining = MathF.Max(DoubleShotRemaining, duration);
                break;
            default:
                RapidFireRemaining = MathF.Max(RapidFireRemaining, duration);
                break;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        if (InvulnerabilityRemaining > 0f && ((int)(InvulnerabilityRemaining * 20f) % 2 == 0))
        {
            return;
        }

        var x = (int)Position.X;
        var y = (int)Position.Y;

        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 16, y + 8, 20, 16), new Color(200, 220, 245));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 4, y + 14, 14, 10), new Color(160, 185, 230));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 34, y + 14, 14, 10), new Color(160, 185, 230));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 23, y + 3, 6, 10), new Color(240, 245, 255));
        PrimitiveRenderer.DrawOutline(spriteBatch, pixel, new Rectangle(x + 16, y + 8, 20, 16), 1, new Color(80, 110, 160));

        if (IsDoubleShotActive)
        {
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 8, y + 10, 3, 6), new Color(110, 245, 255));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 41, y + 10, 3, 6), new Color(110, 245, 255));
        }

        var flameHeight = 6 + (int)(MathF.Abs(MathF.Sin(_enginePulse * 16f)) * 4f);
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 21, y + 24, 4, flameHeight), new Color(255, 105, 60));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 27, y + 24, 4, flameHeight), new Color(255, 145, 65));
    }

    private void Fire(List<Projectile> bullets)
    {
        if (IsDoubleShotActive)
        {
            bullets.Add(new Projectile(new Vector2(Position.X + 12, Position.Y - 12), new Vector2(0f, -510f), false, new Color(120, 255, 170)));
            bullets.Add(new Projectile(new Vector2(Position.X + Size.X - 18, Position.Y - 12), new Vector2(0f, -510f), false, new Color(120, 255, 170)));
            return;
        }

        bullets.Add(new Projectile(new Vector2(Position.X + (Size.X * 0.5f) - 3f, Position.Y - 12), new Vector2(0f, -490f), false, new Color(120, 255, 170)));
    }
}
