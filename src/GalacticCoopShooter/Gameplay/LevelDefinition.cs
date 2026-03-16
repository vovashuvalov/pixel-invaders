using Microsoft.Xna.Framework;

namespace GalacticCoopShooter.Gameplay;

public sealed record LevelDefinition(int Number, string Name, Color PrimaryTint, Color AccentTint, Color LandmarkTint);

public static class CampaignData
{
    public static IReadOnlyList<LevelDefinition> Levels { get; } =
    [
        new LevelDefinition(1, "EARTH ORBIT", new Color(40, 86, 170), new Color(96, 210, 255), new Color(88, 170, 255)),
        new LevelDefinition(2, "MOON", new Color(70, 76, 102), new Color(215, 225, 240), new Color(196, 196, 202)),
        new LevelDefinition(3, "ASTEROIDS", new Color(72, 44, 32), new Color(214, 160, 100), new Color(140, 104, 76)),
        new LevelDefinition(4, "MARS", new Color(110, 42, 34), new Color(255, 140, 88), new Color(185, 78, 60)),
        new LevelDefinition(5, "FINAL FRONT", new Color(96, 18, 46), new Color(255, 86, 122), new Color(255, 118, 86))
    ];
}
