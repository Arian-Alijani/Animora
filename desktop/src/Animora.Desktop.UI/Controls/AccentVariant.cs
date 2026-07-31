namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Accent role shared by every control below with an accent-tint surface (design-reference.md
/// §1.4): StatCard's icon tile and delta badge, StatusChip, CountBadge's optional variant. Member
/// names match the Success/Warning/Danger/Info/Violet modifier classes in
/// Theme/Styles/Chips.axaml and Theme/Styles/Surfaces.axaml exactly, so <see cref="AccentClassing"/>
/// can apply <c>variant.ToString()</c> straight onto a Border's Classes with no lookup table.
/// </summary>
public enum AccentVariant
{
    Success,
    Warning,
    Danger,
    Info,
    Violet,
}
