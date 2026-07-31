using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Status chip (design-reference.md §6). Observed label-to-variant mapping ("پاسخگو"/"تأیید شده" →
/// Success, "در انتظار" → Warning, "لغو شده" → Danger, "اصلاح" → Info) is a module screen's
/// decision, not this control's — it only renders whatever text/variant it is given.
/// </summary>
public partial class StatusChip : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<StatusChip, string?>(nameof(Text));

    public static readonly StyledProperty<AccentVariant> VariantProperty =
        AvaloniaProperty.Register<StatusChip, AccentVariant>(nameof(Variant));

    public StatusChip()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public AccentVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty)
        {
            AccentClassing.Apply(ChipBorder, Variant);
        }
    }
}
