using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Count badge (design-reference.md §6), used on nav items and anywhere else a small numeric/text
/// tag is needed. <see cref="Variant"/> is optional (unlike StatCard/StatusChip's) — leaving it
/// unset keeps the base rule's Brand800 fill the nav rail relies on.
/// </summary>
public partial class CountBadge : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CountBadge, string?>(nameof(Text));

    public static readonly StyledProperty<AccentVariant?> VariantProperty =
        AvaloniaProperty.Register<CountBadge, AccentVariant?>(nameof(Variant));

    public CountBadge()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public AccentVariant? Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty)
        {
            AccentClassing.Apply(BadgeBorder, Variant);
        }
    }
}
