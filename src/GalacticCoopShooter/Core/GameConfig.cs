using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Core;

public static class GameConfig
{
    public const int ScreenWidth = 960;
    public const int ScreenHeight = 720;
    public const int TotalLevels = 5;
    public const int RegularWavesPerLevel = 3;
    public const int StartingLives = 3;
    public const int MaxLives = 5;

    public static readonly Rectangle ScreenBounds = new(0, 0, ScreenWidth, ScreenHeight);
    public static readonly Rectangle PlayerMovementBounds = new(24, 0, ScreenWidth - 48, ScreenHeight);
    public static readonly Rectangle EnemyMovementBounds = new(0, 0, ScreenWidth, ScreenHeight - 72);
}
