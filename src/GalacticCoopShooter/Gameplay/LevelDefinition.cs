using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public sealed record LevelDefinition(int Number, string Name, Color PrimaryTint, Color AccentTint, Color LandmarkTint);

public static class CampaignData
{
    public static IReadOnlyList<LevelDefinition> Levels { get; } =
    [
        new LevelDefinition(1, "EARTH STATION", new Color(34, 62, 120), new Color(128, 216, 255), new Color(86, 168, 238)),
        new LevelDefinition(2, "MOON", new Color(50, 48, 80), new Color(218, 226, 238), new Color(208, 212, 220)),
        new LevelDefinition(3, "RING WORLD", new Color(58, 34, 74), new Color(232, 220, 190), new Color(208, 180, 132)),
        new LevelDefinition(4, "BLUE GIANT", new Color(12, 32, 86), new Color(136, 196, 255), new Color(112, 126, 246)),
        new LevelDefinition(5, "RED FRONT", new Color(66, 18, 30), new Color(255, 118, 72), new Color(236, 74, 40))
    ];
}
