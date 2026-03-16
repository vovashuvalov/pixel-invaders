using GalacticCoopShooter.Core;
using GalacticCoopShooter.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GalacticCoopShooter.Entities;

public sealed class Player : Entity
{
    private const float MoveSpeed = 360f;
    private const float BaseFireCooldown = 0.18f;
    private const float TripleShotCooldown = 0.14f;

    private float _fireCooldownRemaining;
    private float _enginePulse;

    public Player(Vector2 position)
        : base(position, new Vector2(52f, 30f))
    {
    }

    public int Lives { get; private set; } = GameConfig.StartingLives;
    public float TripleShotRemaining { get; private set; }
    public float ShieldRemaining { get; private set; }
    public float DamageRecoveryRemaining { get; private set; }

    public bool IsTripleShotActive => TripleShotRemaining > 0f;
    public bool IsShieldActive => ShieldRemaining > 0f;
    public Vector2 Center => Position + (Size * 0.5f);

    public void Tick(float deltaTime, InputManager input, Rectangle movementBounds, List<Projectile> bullets)
    {
        if (!IsActive)
        {
            return;
        }

        _enginePulse += deltaTime;

        TripleShotRemaining = MathF.Max(0f, TripleShotRemaining - deltaTime);
        ShieldRemaining = MathF.Max(0f, ShieldRemaining - deltaTime);
        DamageRecoveryRemaining = MathF.Max(0f, DamageRecoveryRemaining - deltaTime);
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

        if (input.IsFireHeld() && _fireCooldownRemaining <= 0f)
        {
            Fire(bullets);
            _fireCooldownRemaining = IsTripleShotActive ? TripleShotCooldown : BaseFireCooldown;
        }
    }

    public bool TryTakeDamage()
    {
        if (IsShieldActive || DamageRecoveryRemaining > 0f)
        {
            return false;
        }

        Lives = Math.Max(0, Lives - 1);
        DamageRecoveryRemaining = 1.1f;
        return true;
    }

    public void ApplyPowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.ExtraLife:
                Lives = Math.Min(GameConfig.MaxLives, Lives + 1);
                break;
            case PowerUpType.TripleShot:
                TripleShotRemaining = MathF.Max(TripleShotRemaining, duration);
                break;
            case PowerUpType.Shield:
                ShieldRemaining = MathF.Max(ShieldRemaining, duration);
                break;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!IsActive)
        {
            return;
        }

        if (!IsShieldActive && DamageRecoveryRemaining > 0f && ((int)(DamageRecoveryRemaining * 20f) % 2 == 0))
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

        if (IsTripleShotActive)
        {
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 8, y + 10, 3, 6), new Color(110, 245, 255));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 24, y + 5, 3, 6), new Color(110, 245, 255));
            PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 41, y + 10, 3, 6), new Color(110, 245, 255));
        }

        var flameHeight = 6 + (int)(MathF.Abs(MathF.Sin(_enginePulse * 16f)) * 4f);
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 21, y + 24, 4, flameHeight), new Color(255, 105, 60));
        PrimitiveRenderer.DrawRect(spriteBatch, pixel, new Rectangle(x + 27, y + 24, 4, flameHeight), new Color(255, 145, 65));

        if (IsShieldActive)
        {
            PrimitiveRenderer.DrawOutline(spriteBatch, pixel, new Rectangle(x - 4, y - 4, (int)Size.X + 8, (int)Size.Y + 8), 2, new Color(130, 255, 215));
        }
    }

    private void Fire(List<Projectile> bullets)
    {
        if (IsTripleShotActive)
        {
            bullets.Add(new Projectile(new Vector2(Position.X + 10, Position.Y - 12), new Vector2(-110f, -520f), ProjectileType.PlayerShot));
            bullets.Add(new Projectile(new Vector2(Position.X + (Size.X * 0.5f) - 2f, Position.Y - 14), new Vector2(0f, -560f), ProjectileType.PlayerShot));
            bullets.Add(new Projectile(new Vector2(Position.X + Size.X - 14, Position.Y - 12), new Vector2(110f, -520f), ProjectileType.PlayerShot));
            return;
        }

        bullets.Add(new Projectile(new Vector2(Position.X + (Size.X * 0.5f) - 2f, Position.Y - 12), new Vector2(0f, -540f), ProjectileType.PlayerShot));
    }
}
