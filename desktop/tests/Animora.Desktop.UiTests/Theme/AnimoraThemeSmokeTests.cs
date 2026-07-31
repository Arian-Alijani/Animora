using System.Linq;
using Animora.Desktop.UI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAssertions;

namespace Animora.Desktop.UiTests.Theme;

/// <summary>
/// Headless RTL smoke test for the whole design system (PHASE.md criterion 2, item 33): a
/// per-screen equivalent of this arrives with every real screen (desktop screen DoD), but the
/// design system itself has no screen of its own to carry it, so the phase gets one test host here
/// instead.
/// </summary>
public class AnimoraThemeSmokeTests
{
    [AvaloniaFact]
    public void Shell_root_resolves_RightToLeft_flow_direction_from_AnimoraTheme()
    {
        var window = new Window();
        window.Show();

        window.FlowDirection.Should().Be(FlowDirection.RightToLeft);
    }

    [AvaloniaFact]
    public void Shell_root_resolves_Vazirmatn_as_the_effective_default_font_family()
    {
        var window = new Window();
        window.Show();

        var expected = (FontFamily)GetResource(window, "FontFamilyDefault")!;
        window.FontFamily.Should().Be(expected);
    }

    [AvaloniaFact]
    public void StatCard_resolves_its_token_brushes_from_AnimoraTheme()
    {
        var window = new Window();
        var statCard = new StatCard
        {
            IconGlyph = "mdi-account-group",
            IconAccent = AccentVariant.Info,
            MetricValue = "۱,۲۸۴",
            Label = "بیماران",
        };
        window.Content = statCard;
        window.Show();

        var metricValue = statCard.GetVisualDescendants().OfType<TextBlock>()
            .First(block => block.Classes.Contains("MetricValue"));
        metricValue.Foreground.Should().BeSameAs(GetResource(window, "BrushTextPrimary"));

        var iconTile = statCard.GetVisualDescendants().OfType<Border>()
            .First(border => border.Classes.Contains("TintedAccent"));
        iconTile.Background.Should().BeSameAs(GetResource(window, "BrushAccentInfoTint"));
    }

    [AvaloniaFact]
    public void StatusChip_resolves_its_token_brushes_from_AnimoraTheme()
    {
        var window = new Window();
        var statusChip = new StatusChip { Text = "لغو شده", Variant = AccentVariant.Danger };
        window.Content = statusChip;
        window.Show();

        var chipBorder = statusChip.GetVisualDescendants().OfType<Border>()
            .First(border => border.Classes.Contains("Chip"));
        chipBorder.Classes.Should().Contain("Danger");
        chipBorder.Background.Should().BeSameAs(GetResource(window, "BrushAccentDangerTint"));

        var label = statusChip.GetVisualDescendants().OfType<TextBlock>().First();
        label.Foreground.Should().BeSameAs(GetResource(window, "BrushAccentDangerStrong"));
    }

    // AnimoraTheme.axaml (item 12) merges every token/style file into Application.Styles rather
    // than Application.Resources, so a resolved control's own tree is the reliable place to look a
    // token brush up from (Window is the nearest IResourceHost with the full merged chain behind it).
    private static object? GetResource(Window window, string key) =>
        window.TryFindResource(key, out object? value) ? value : null;
}
