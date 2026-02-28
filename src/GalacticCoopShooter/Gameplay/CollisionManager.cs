using GalacticCoopShooter.Entities;
using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public readonly record struct CollisionReport(int ScoreDelta, int HitsTaken, int PowerUpsCollected);

public sealed class CollisionManager
{
    public CollisionReport Resolve(
        Player player,
        List<Projectile> playerBullets,
        List<Projectile> enemyEggs,
        IReadOnlyList<Enemy> enemies,
        List<PowerUp> powerUps,
        Random random)
    {
        var scoreDelta = 0;
        var hitsTaken = 0;
        var powerUpsCollected = 0;

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

                    if (random.NextSingle() < 0.2f)
                    {
                        var type = random.Next(0, 2) == 0 ? PowerUpType.RapidFire : PowerUpType.DoubleShot;
                        var position = new Vector2(enemy.Position.X + (enemy.Size.X * 0.5f) - 11f, enemy.Position.Y + (enemy.Size.Y * 0.5f) - 11f);
                        powerUps.Add(new PowerUp(type, position));
                    }
                }

                break;
            }
        }

        for (var i = 0; i < enemyEggs.Count; i++)
        {
            var egg = enemyEggs[i];

            if (!egg.IsActive || !egg.Bounds.Intersects(player.Bounds))
            {
                continue;
            }

            egg.IsActive = false;

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

            player.ApplyPowerUp(powerUp.Type, powerUp.DurationSeconds);
            powerUp.IsActive = false;
            powerUpsCollected++;
        }

        return new CollisionReport(scoreDelta, hitsTaken, powerUpsCollected);
    }
}
