using GalacticCoopShooter.Entities;
using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public readonly record struct CollisionReport(int ScoreDelta, int HitsTaken, IReadOnlyList<PowerUpType> CollectedPowerUps);

public sealed class CollisionManager
{
    public CollisionReport Resolve(
        Player player,
        List<Projectile> playerBullets,
        List<Projectile> enemyProjectiles,
        IReadOnlyList<Enemy> enemies,
        List<PowerUp> powerUps,
        Random random)
    {
        var scoreDelta = 0;
        var hitsTaken = 0;
        var collectedPowerUps = new List<PowerUpType>();

        for (var i = 0; i < playerBullets.Count; i++)
        {
            var bullet = playerBullets[i];

            if (!bullet.IsActive)
            {
                continue;
            }

            for (var j = 0; j < enemies.Count; j++)
            {
                var enemy = enemies[j];

                if (!enemy.IsActive)
                {
                    continue;
                }

                if (!bullet.Bounds.Intersects(enemy.Bounds))
                {
                    continue;
                }

                bullet.IsActive = false;

                var destroyed = enemy.TakeDamage(1);

                if (destroyed)
                {
                    scoreDelta += enemy.ScoreValue;

                    if (random.NextSingle() < 0.24f)
                    {
                        var type = RollPowerUp(random);
                        var position = new Vector2(enemy.Position.X + (enemy.Size.X * 0.5f) - 13f, enemy.Position.Y + (enemy.Size.Y * 0.5f) - 13f);
                        powerUps.Add(new PowerUp(type, position));
                    }
                }

                break;
            }
        }

        for (var i = 0; i < enemyProjectiles.Count; i++)
        {
            var projectile = enemyProjectiles[i];

            if (!projectile.IsActive || !projectile.Bounds.Intersects(player.Bounds))
            {
                continue;
            }

            projectile.IsActive = false;

            if (player.TryTakeDamage())
            {
                hitsTaken++;
            }
        }

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];

            if (!enemy.IsActive || !enemy.Bounds.Intersects(player.Bounds))
            {
                continue;
            }

            enemy.IsActive = false;

            if (player.TryTakeDamage())
            {
                hitsTaken++;
            }
        }

        for (var i = 0; i < powerUps.Count; i++)
        {
            var powerUp = powerUps[i];

            if (!powerUp.IsActive || !powerUp.Bounds.Intersects(player.Bounds))
            {
                continue;
            }

            collectedPowerUps.Add(powerUp.Type);
            powerUp.IsActive = false;
        }

        return new CollisionReport(scoreDelta, hitsTaken, collectedPowerUps);
    }

    private static PowerUpType RollPowerUp(Random random)
    {
        var roll = random.NextSingle();

        if (roll < 0.12f)
        {
            return PowerUpType.ExtraLife;
        }

        if (roll < 0.48f)
        {
            return PowerUpType.TripleShot;
        }

        if (roll < 0.68f)
        {
            return PowerUpType.Bomb;
        }

        return PowerUpType.Shield;
    }
}
