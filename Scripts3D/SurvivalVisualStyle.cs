using Godot;

// Shared presentation values. No gameplay rules or mutable round data here.
public static class SurvivalVisualStyle
{
    public static readonly Color Paper = new("f1f2e9");
    public static readonly Color Ink = new("171d25");
    public static readonly Color Muted = new("69716f");
    public static readonly Color Accent = new("2254de");
    public static readonly Color Danger = new("d93e24");
    public static readonly Color Track = new("d2d7cc");
    public static readonly Color Pressed = new("153493");
    public static readonly Color Floor = new("8d9c82");
    public static readonly Color Wall = new("adbca1");
    public static readonly Color TileA = new("b0c2a4");
    public static readonly Color TileB = new("9eb092");
    public static readonly Color ArenaEdge = new("e6ed34");
    public static readonly Color Player = new(0.08f, 0.25f, 0.9f);
    public static readonly Color AimMarker = new(0.06f, 0.12f, 0.25f);
    public const int ClockFontSize = 96;
    public const int ResultFontSize = 72;
    public const double CriticalSeconds = 10;
    public const float CriticalHealthRatio = 0.3f;
    public const float ResultOpacity = 0.94f;
    public const float AmbientEnergy = 0.4f;
    public const float DirectionalEnergy = 0.45f;

    public static Font BodyFont() => new SystemFont { FontNames = new[] { "Bahnschrift", "Arial" } };
    public static Font BoldFont() => new SystemFont { FontNames = new[] { "Arial", "Bahnschrift" }, FontWeight = 700 };
}
