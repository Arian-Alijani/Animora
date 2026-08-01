using Animora.Desktop.App.Composition;
using Animora.Desktop.App.Shell;
using Animora.Desktop.Modules.Reporting.ViewModels;
using Animora.Desktop.Modules.Reporting.Views;
using Animora.Desktop.UI.Controls;
using Animora.Desktop.UI.Navigation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingRoutes = Animora.Desktop.Modules.Reporting.Composition.ServiceCollectionExtensions;

namespace Animora.Desktop.UiTests.Shell;

/// <summary>
/// The shell's headless RTL smoke test (PHASE.md criteria 1-3, extensibility playbook step 5) and the
/// template every screen phase copies. It asserts against the *real* composition root — the same
/// <c>AddDesktopApp()</c> the app boots with — so a broken route registration, a missing token or a
/// per-screen flow-direction regression fails here rather than on a Windows host.
/// <para>
/// The route it expects is the Reporting module's Home route, reached only through the registry: this
/// file names the module because a test may, while the shell project itself may not (DESK-ARCH-05,
/// guarded by <c>ArchTests/ShellDecouplingRules</c>).
/// </para>
/// </summary>
public class ShellWindowSmokeTests
{
    [AvaloniaFact]
    public void Shell_root_is_RightToLeft_and_the_hosted_screen_inherits_it()
    {
        using ServiceProvider services = BuildAppServices();

        ShellWindow shell = ShowShell(services);

        // Set once by AnimoraTheme's root style and inherited from there down; neither ShellWindow.axaml
        // nor HomeView.axaml sets FlowDirection, which is exactly what this pair of assertions protects
        // (DT-06, DESK-ARCH-06).
        shell.FlowDirection.Should().Be(FlowDirection.RightToLeft);
        HostedScreen<HomeView>(shell).FlowDirection.Should().Be(FlowDirection.RightToLeft);
    }

    [AvaloniaFact]
    public void Rail_lists_the_route_the_reporting_module_registered()
    {
        using ServiceProvider services = BuildAppServices();

        ShellWindow shell = ShowShell(services);

        ShellRailItemViewModel[] railItems =
        [
            .. shell.GetVisualDescendants()
                .OfType<Button>()
                .Where(button => button.Classes.Contains("NavItem"))
                .Select(button => button.DataContext)
                .OfType<ShellRailItemViewModel>()
        ];

        // One rail pill, for the one route registered in this phase: the shell rendered a screen entry
        // it has no compile-time knowledge of.
        railItems.Should().ContainSingle();
        ShellRailItemViewModel homeItem = railItems[0];
        homeItem.RouteKey.Should().Be(ReportingRoutes.HomeRouteKey);
        homeItem.IsActive.Should().BeTrue("the landing route's pill is the active one after startup");

        // Label and group heading come from the descriptor and the shell's own text surface — compared
        // against those sources rather than against literals, so a wording change stays a one-line edit.
        RenderedText(shell, "NavLabel").Should().Contain(homeItem.Title);
        RenderedText(shell, "Overline").Should().Contain(ShellText.CommandCenterGroup);
    }

    [AvaloniaFact]
    public void Status_indicator_is_present_and_shows_the_placeholder_online_state()
    {
        using ServiceProvider services = BuildAppServices();

        ShellWindow shell = ShowShell(services);

        // Persistent and non-blocking by construction: a chip in the top bar, no dialog, no command
        // (DESK-ARCH-07/08, DT-10).
        StatusIndicator indicator = shell.GetVisualDescendants().OfType<StatusIndicator>().Single();

        StatusChip connectivity = Named<StatusChip>(indicator, "ConnectivityChip");
        connectivity.IsVisible.Should().BeTrue();
        connectivity.Text.Should().Be(ShellText.StatusOnline);

        // The licensing chip only appears once writes are actually withdrawn (LIC-12); the phase-02
        // placeholder never is.
        Named<StatusChip>(indicator, "ReadOnlyChip").IsVisible.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Navigating_the_default_route_renders_HomeView_in_the_content_region()
    {
        using ServiceProvider services = BuildAppServices();

        ShellWindow shell = ShowShell(services);

        ContentControl contentRegion = Named<ContentControl>(shell, "ContentRegion");
        contentRegion.Content.Should().BeOfType<HomeView>();

        // The screen is wired to its own data seam end to end: the view model dispatched its query
        // through Mediator and the Stage A read store answered it, without the shell knowing any of it.
        HostedScreen<HomeView>(shell).DataContext.Should()
            .BeOfType<HomeViewModel>()
            .Which.Summary.Should().NotBeNull();

        // Top bar reads the same descriptor the rail does, so the page title needs no per-screen copy.
        string registeredTitle = services.GetRequiredService<IRouteRegistry>().RailEntries[0].Title;
        Named<TextBlock>(shell, "PageTitleText").Text.Should().Be(registeredTitle);
        Named<TextBlock>(shell, "BreadcrumbText").Text.Should().EndWith(registeredTitle);
    }

    private static ServiceProvider BuildAppServices() =>
        new ServiceCollection().AddDesktopApp().BuildServiceProvider();

    // StartupSequence owns these two steps in the app, but it is internal and builds a generic host,
    // so its shell stage is replayed here in the same order: landing route resolved from the registry
    // first, window shown second — never the reverse, or the content region flashes empty.
    private static ShellWindow ShowShell(IServiceProvider services)
    {
        ShellWindow shell = services.GetRequiredService<ShellWindow>();
        services.GetRequiredService<ShellViewModel>().ShowDefaultRoute();
        shell.Show();

        // Show() runs the initial layout pass; this drains anything the landing screen's load command
        // posted to the dispatcher, so the assertions below see the state the first painted frame
        // would — without any test ever awaiting that load (DESK-ARCH-10/16).
        Dispatcher.UIThread.RunJobs();

        return shell;
    }

    // Window.Show() runs the initial layout pass, so templates are applied and the hosted screen is
    // attached (and therefore inheriting) by the time any of these lookups run.
    private static TScreen HostedScreen<TScreen>(Visual shell)
        where TScreen : Control =>
        shell.GetVisualDescendants().OfType<TScreen>().Single();

    private static TControl Named<TControl>(Visual root, string name)
        where TControl : Control
    {
        TControl? found = root.GetVisualDescendants()
            .OfType<TControl>()
            .FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));

        found.Should().NotBeNull($"the shell is expected to render a {typeof(TControl).Name} named '{name}'");

        return found!;
    }

    private static IEnumerable<string?> RenderedText(Visual root, string styleClass) =>
        root.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(block => block.Classes.Contains(styleClass))
            .Select(block => block.Text);
}
