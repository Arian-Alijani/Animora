using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// KPI stat card (design-reference.md §6): icon tile, delta badge, metric value, label, optional
/// footer note. A module screen sets only the data properties below; every color and geometry
/// value comes from the Card/TintedAccent/Chip token-driven style classes in Theme/Styles/.
/// </summary>
public partial class StatCard : UserControl
{
    public static readonly StyledProperty<string?> IconGlyphProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(IconGlyph));

    public static readonly StyledProperty<AccentVariant> IconAccentProperty =
        AvaloniaProperty.Register<StatCard, AccentVariant>(nameof(IconAccent));

    public static readonly StyledProperty<string?> MetricValueProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(MetricValue));

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Label));

    public static readonly StyledProperty<string?> DeltaTextProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(DeltaText));

    public static readonly StyledProperty<DeltaDirection> DeltaDirectionProperty =
        AvaloniaProperty.Register<StatCard, DeltaDirection>(nameof(DeltaDirection));

    public static readonly StyledProperty<string?> FooterNoteProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(FooterNote));

    public StatCard()
    {
        InitializeComponent();
    }

    /// <summary>Material Design icon key (e.g. <c>mdi-account-group</c>) for the icon tile.</summary>
    public string? IconGlyph
    {
        get => GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>Icon tile tint/glyph accent. Defaults to Success.</summary>
    public AccentVariant IconAccent
    {
        get => GetValue(IconAccentProperty);
        set => SetValue(IconAccentProperty, value);
    }

    /// <summary>Already-formatted metric text (Jalali/number formatting happens upstream, CONV-05/07).</summary>
    public string? MetricValue
    {
        get => GetValue(MetricValueProperty);
        set => SetValue(MetricValueProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Already-formatted delta text (e.g. "12%"). Ignored while <see cref="DeltaDirection"/> is Neutral.</summary>
    public string? DeltaText
    {
        get => GetValue(DeltaTextProperty);
        set => SetValue(DeltaTextProperty, value);
    }

    /// <summary>Drives the delta badge's visibility, arrow glyph and accent (Up = success, Down = danger).</summary>
    public DeltaDirection DeltaDirection
    {
        get => GetValue(DeltaDirectionProperty);
        set => SetValue(DeltaDirectionProperty, value);
    }

    /// <summary>Optional footer line under a Divider. Hidden entirely when null or empty.</summary>
    public string? FooterNote
    {
        get => GetValue(FooterNoteProperty);
        set => SetValue(FooterNoteProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IconAccentProperty)
        {
            AccentClassing.Apply(IconTileBorder, IconAccent);
        }
        else if (change.Property == DeltaDirectionProperty)
        {
            ApplyDeltaDirection(DeltaDirection);
        }
        else if (change.Property == FooterNoteProperty)
        {
            FooterPanel.IsVisible = !string.IsNullOrEmpty(FooterNote);
        }
    }

    private void ApplyDeltaDirection(DeltaDirection direction)
    {
        DeltaBadgeBorder.IsVisible = direction != DeltaDirection.Neutral;
        AccentClassing.Apply(DeltaBadgeBorder, direction == DeltaDirection.Down ? AccentVariant.Danger : AccentVariant.Success);
        DeltaGlyphIcon.Value = direction == DeltaDirection.Down ? "mdi-arrow-down" : "mdi-arrow-up";
    }
}
