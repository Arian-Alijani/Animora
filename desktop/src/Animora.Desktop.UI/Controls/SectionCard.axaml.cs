using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Animora.Desktop.UI.Controls;

/// <summary>
/// Section card (design-reference.md §6): title/eyebrow header, optional trailing action link,
/// free-form body via <see cref="BodyContent"/>. <see cref="ActionCommand"/> is a plain
/// <see cref="ICommand"/> rather than a Mediator request — Button.Command works with any
/// implementation, and this project has no ViewModelBase yet (item 29).
/// </summary>
public partial class SectionCard : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SectionCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> EyebrowProperty =
        AvaloniaProperty.Register<SectionCard, string?>(nameof(Eyebrow));

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<SectionCard, string?>(nameof(ActionText));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<SectionCard, ICommand?>(nameof(ActionCommand));

    public static readonly StyledProperty<object?> ActionCommandParameterProperty =
        AvaloniaProperty.Register<SectionCard, object?>(nameof(ActionCommandParameter));

    public static readonly StyledProperty<object?> BodyContentProperty =
        AvaloniaProperty.Register<SectionCard, object?>(nameof(BodyContent));

    public SectionCard()
    {
        InitializeComponent();
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional Overline label above the title. Hidden entirely when null or empty.</summary>
    public string? Eyebrow
    {
        get => GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    /// <summary>Optional trailing action link text. The link is hidden entirely when null or empty.</summary>
    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    /// <summary>The card's body — chart, list or table — set via the property-element syntax
    /// documented in SectionCard.axaml.</summary>
    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EyebrowProperty)
        {
            EyebrowText.IsVisible = !string.IsNullOrEmpty(Eyebrow);
        }
        else if (change.Property == ActionTextProperty)
        {
            ActionButton.IsVisible = !string.IsNullOrEmpty(ActionText);
        }
    }
}
