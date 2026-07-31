namespace Animora.Desktop.UI.Controls;

/// <summary>
/// StatCard delta-badge direction (design-reference.md §6, "delta badge ... ▲/▼ glyph"). Neutral
/// hides the badge outright — the reference never shows a KPI card with no known trend.
/// </summary>
public enum DeltaDirection
{
    Neutral,
    Up,
    Down,
}
