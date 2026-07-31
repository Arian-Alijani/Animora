using Animora.Desktop.UI.Services;
using Animora.Desktop.UiTests;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Animora.Desktop.UiTests;

/// <summary>
/// Minimal headless host for this project's <c>[AvaloniaFact]</c> tests. Loads
/// <c>avares://Animora.Desktop.UI/Theme/AnimoraTheme.axaml</c> the same way
/// <c>Animora.Desktop.App/App.axaml</c> will (item 12: the single include point), so a test failure
/// here means a real app failure, not a test-only wiring gap.
/// </summary>
public sealed class TestApp : Application
{
    public override void Initialize()
    {
        IconProviderRegistrar.Register();

        Styles.Add(new StyleInclude(new Uri("avares://Animora.Desktop.UiTests/"))
        {
            Source = new Uri("avares://Animora.Desktop.UI/Theme/AnimoraTheme.axaml"),
        });
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
