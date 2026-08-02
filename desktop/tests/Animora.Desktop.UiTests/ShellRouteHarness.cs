using Animora.Desktop.App.Composition;
using Animora.Desktop.App.Shell;
using Animora.Desktop.UI.Navigation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;

namespace Animora.Desktop.UiTests;

/// <summary>
/// One live shell, driven the way the app drives it, for the per-screen RTL smoke tests (desktop
/// screen DoD, playbook step 5): <c>AddDesktopApp()</c> is built, the shell is shown at its default
/// route, and every hop after that goes through <see cref="INavigationService"/> — never a View or
/// ViewModel constructed by hand — so a screen's own regression can never mask a broken
/// shell/registry wiring underneath it.
/// <para>
/// A harness instance outlives a single navigation on purpose: a screen reached only with a record
/// id (a medical file, an owner-scoped list) needs the id its list screen just rendered, and
/// re-reading it from the seeded store directly would test a path no user takes.
/// </para>
/// </summary>
internal sealed class ShellRouteHarness : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly ShellWindow _shell;

    private ShellRouteHarness(ServiceProvider services, ShellWindow shell)
    {
        _services = services;
        _shell = shell;
    }

    /// <summary>
    /// Replays <c>StartupSequence</c>'s shell stage in its own order — landing route resolved from
    /// the registry first, window shown second — the same way <c>Shell/ShellWindowSmokeTests</c>
    /// does, since that type is internal and builds a generic host.
    /// </summary>
    public static ShellRouteHarness Start()
    {
        ServiceProvider services = new ServiceCollection().AddDesktopApp().BuildServiceProvider();

        ShellWindow shell = services.GetRequiredService<ShellWindow>();
        services.GetRequiredService<ShellViewModel>().ShowDefaultRoute();
        shell.Show();

        // Show() runs the initial layout pass; this drains what the landing screen's load command
        // posted to the dispatcher rather than awaited (DESK-ARCH-10/16).
        Dispatcher.UIThread.RunJobs();

        return new ShellRouteHarness(services, shell);
    }

    /// <summary>Navigates to <paramref name="routeKey"/> and returns the control the shell put in
    /// its content region, with the target screen's own load command already drained.</summary>
    public Control NavigateTo(string routeKey, object? parameter = null)
    {
        _services.GetRequiredService<INavigationService>().NavigateTo(routeKey, parameter);

        Dispatcher.UIThread.RunJobs();

        ContentControl contentRegion = _shell.GetVisualDescendants().OfType<ContentControl>()
            .Single(control => string.Equals(control.Name, "ContentRegion", StringComparison.Ordinal));

        return (Control)contentRegion.Content!;
    }

    public void Dispose() => _services.Dispose();
}
