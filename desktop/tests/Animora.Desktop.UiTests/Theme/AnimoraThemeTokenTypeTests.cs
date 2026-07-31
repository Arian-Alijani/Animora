using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using FluentAssertions;

namespace Animora.Desktop.UiTests.Theme;

/// <summary>
/// Guards the two ways a token can compile cleanly and still throw at window-construction time,
/// both of which slipped past <see cref="AnimoraThemeSmokeTests"/> (it resolves brushes only, never
/// the geometry tokens and never a DataGrid): compiled XAML casts a <c>{StaticResource ...}</c>
/// straight to the target property's type with no numeric conversion, so a token meant for
/// <c>Padding</c>/<c>BorderThickness</c> but stored as <c>x:Double</c> throws
/// <see cref="System.InvalidCastException"/>; and a control whose theme lives in a package that is
/// referenced but never included gets no template at all.
/// </summary>
public class AnimoraThemeTokenTypeTests
{
    [AvaloniaFact]
    public void Padding_tokens_resolve_as_Thickness_not_as_a_scalar()
    {
        // Every Theme/Tokens/Layout.axaml key a consumer is expected to bind straight to a
        // Thickness-typed property (Padding, BorderThickness, Margin).
        string[] thicknessTokenKeys =
        [
            "ContentPadding",
            "RailPadding",
            "ButtonPaddingX",
            "ButtonPaddingXLarge",
            "CardPadding",
            "CardPaddingLarge",
            "StrokeThickness",
            "ChipPaddingX",
            "CountBadgePaddingX",
            "CalloutPadding",
            "DataGridCellPadding",
        ];

        var window = new Window();
        window.Show();

        foreach (var key in thicknessTokenKeys)
        {
            window.TryFindResource(key, out object? value).Should().BeTrue(
                "token '{0}' must exist in AnimoraTheme", key);
            value.Should().BeOfType<Thickness>(
                "token '{0}' is bound straight to a Thickness-typed property", key);
        }
    }

    [AvaloniaFact]
    public void DataGrid_resolves_a_control_theme_from_AnimoraTheme()
    {
        var window = new Window();
        var dataGrid = new DataGrid();
        window.Content = dataGrid;
        window.Show();

        dataGrid.Template.Should().NotBeNull();
    }
}
