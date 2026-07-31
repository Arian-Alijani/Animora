using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Empty-state placeholder, not part of the observed reference screens (§6 covers only populated
/// states) but needed by every list/table/report module screen (04-12) for the zero-rows case.
/// Built from the same token set (BodyStrong/Caption/TextMuted, Spacing12) so it matches the rest
/// of the design system without a dedicated reference value.
/// </summary>
public partial class EmptyState : UserControl
{
    public static readonly StyledProperty<string?> IconGlyphProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(IconGlyph), "mdi-tray-arrow-down");

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<EmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<EmptyState, object?>(nameof(ActionContent));

    public EmptyState()
    {
        InitializeComponent();
    }

    public string? IconGlyph
    {
        get => GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional supporting line under the title. Hidden entirely when null or empty.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Optional action (e.g. a Button) shown below the description. Hidden when null.</summary>
    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DescriptionProperty)
        {
            DescriptionText.IsVisible = !string.IsNullOrEmpty(Description);
        }
        else if (change.Property == ActionContentProperty)
        {
            ActionPresenter.IsVisible = ActionContent is not null;
        }
    }
}
