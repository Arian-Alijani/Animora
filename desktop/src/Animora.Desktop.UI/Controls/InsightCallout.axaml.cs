using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>Insight callout (design-reference.md §6): a leading glyph plus one Caption line.</summary>
public partial class InsightCallout : UserControl
{
    public static readonly StyledProperty<string?> IconGlyphProperty =
        AvaloniaProperty.Register<InsightCallout, string?>(nameof(IconGlyph), "mdi-lightbulb-on");

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<InsightCallout, string?>(nameof(Message));

    public InsightCallout()
    {
        InitializeComponent();
    }

    public string? IconGlyph
    {
        get => GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}
