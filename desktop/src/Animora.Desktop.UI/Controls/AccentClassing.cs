using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Swaps the accent modifier class (Success/Warning/Danger/Info/Violet) on a Border that already
/// carries a base class from Theme/Styles/ (Chip, TintedAccent, CountBadge). Avalonia has no
/// bindable enum-to-Classes shortcut, and the actual colors stay in those style files either way
/// (DT-06) — every accent-variant control in this folder funnels its class swap through here
/// instead of repeating the remove/add pair.
/// </summary>
internal static class AccentClassing
{
    private static readonly string[] VariantClassNames =
    {
        nameof(AccentVariant.Success),
        nameof(AccentVariant.Warning),
        nameof(AccentVariant.Danger),
        nameof(AccentVariant.Info),
        nameof(AccentVariant.Violet),
    };

    /// <param name="target">The Border already carrying a base style class (Chip, TintedAccent,
    /// CountBadge) whose accent modifier class is being swapped.</param>
    /// <param name="variant">
    /// <see langword="null"/> clears every variant class, leaving the target on its base rule's
    /// default look (used by CountBadge, whose Variant is optional).
    /// </param>
    public static void Apply(Border target, AccentVariant? variant)
    {
        foreach (string className in VariantClassNames)
        {
            target.Classes.Remove(className);
        }

        if (variant is { } value)
        {
            target.Classes.Add(value.ToString());
        }
    }
}
