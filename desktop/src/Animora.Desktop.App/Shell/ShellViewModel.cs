using System.Windows.Input;
using Animora.Desktop.UI.AppState;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;

namespace Animora.Desktop.App.Shell;

/// <summary>
/// The single-window shell's own view model (DESK-ARCH-01): it reads the rail from
/// <see cref="IRouteRegistry"/>, asks <see cref="INavigationService"/> to switch screens, and hosts
/// whatever view the navigation service hands back — so the shell renders module screens it has no
/// compile-time knowledge of (DESK-ARCH-05, DT-09).
/// <para>
/// Cross-screen state is consumed through the injected app-state singletons
/// (<see cref="CurrentUser"/>, <see cref="Status"/>) rather than copied onto this type, which is what
/// keeps DESK-ARCH-03 intact even for the one view model every screen sits inside.
/// </para>
/// <para>
/// Property change notifications are raised through <c>SetProperty</c> by hand: the
/// <c>CommunityToolkit.Mvvm</c> source generator is an analyzer asset of a package this project only
/// sees transitively, so <c>[ObservableProperty]</c> is not available here (same reason as
/// <c>AppState/AppStatusState</c>).
/// </para>
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    // Coarsest crumb first, reading right to left from the inherited flow direction
    // (design-reference §6/§7) — the separator is plain text, not a per-crumb control, because every
    // crumb shares one Caption/TextMuted style.
    private const string BreadcrumbSeparator = " / ";

    private readonly INavigationService _navigation;
    private readonly ShellRailItemViewModel[] _railItems;

    private object? _currentContent;
    private string _pageTitle = string.Empty;
    private string _breadcrumb = string.Empty;

    public ShellViewModel(
        IRouteRegistry routeRegistry,
        INavigationService navigation,
        ICurrentUserState currentUser,
        IAppStatusState appStatus)
    {
        _navigation = navigation;
        CurrentUser = currentUser;
        Status = new ShellStatusViewModel(appStatus);
        NavigateCommand = new RelayCommand<string>(Navigate);

        // IRouteRegistry.RailEntries is already ordered by group then rail order, so GroupBy's
        // first-appearance grouping reproduces that order without re-sorting here.
        RailGroups =
        [
            .. routeRegistry.RailEntries
                .GroupBy(entry => entry.RailGroup)
                .Select(group => CreateRailGroup(GroupTitle(group.Key), group))
        ];
        _railItems = [.. RailGroups.SelectMany(group => group.Items)];

        // Singleton-to-singleton for the life of the process; there is no detach point, and the shell
        // must keep reacting to navigations raised by any screen, not just by its own rail.
        _navigation.RouteChanged += OnRouteChanged;
    }

    /// <summary>Display-only user state for the top bar's chip (DESK-ARCH-03).</summary>
    public ICurrentUserState CurrentUser { get; }

    /// <summary>Backing state for the persistent status indicator (DESK-ARCH-07/08).</summary>
    public ShellStatusViewModel Status { get; }

    /// <summary>The one navigate command every rail pill binds to, with the route key as its parameter.</summary>
    public ICommand NavigateCommand { get; }

    public IReadOnlyList<ShellRailGroupViewModel> RailGroups { get; }

    /// <summary>
    /// The active screen's view, hosted by the shell's content region. Typed as <see cref="object"/>
    /// because the shell only forwards a control it neither names nor constructs — the route
    /// descriptor built it (DESK-ARCH-01/05).
    /// </summary>
    public object? CurrentContent
    {
        get => _currentContent;
        private set => SetProperty(ref _currentContent, value);
    }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public string Breadcrumb
    {
        get => _breadcrumb;
        private set => SetProperty(ref _breadcrumb, value);
    }

    /// <summary>
    /// Shows the first rail route as the landing screen. Called by the startup sequence once the shell
    /// window exists, rather than from the constructor, so navigation is an explicit startup stage
    /// (DESK-ARCH-16) instead of a side effect of composing the container. A registry with no
    /// rail-visible route (a test host that registers none) leaves the content region empty.
    /// </summary>
    public void ShowDefaultRoute()
    {
        if (_railItems.Length > 0)
        {
            _navigation.NavigateTo(_railItems[0].RouteKey);
        }
    }

    private static string GroupTitle(RailGroup group) => group switch
    {
        RailGroup.ClinicManagement => ShellText.ClinicManagementGroup,
        RailGroup.FinancialOperations => ShellText.FinancialOperationsGroup,
        _ => ShellText.CommandCenterGroup,
    };

    private ShellRailGroupViewModel CreateRailGroup(string title, IEnumerable<NavigationEntry> entries) =>
        new(title, [.. entries.Select(entry => new ShellRailItemViewModel(entry, title, NavigateCommand))]);

    private void Navigate(string? routeKey)
    {
        if (!string.IsNullOrWhiteSpace(routeKey))
        {
            _navigation.NavigateTo(routeKey);
        }
    }

    private void OnRouteChanged(object? sender, RouteChangedEventArgs e)
    {
        CurrentContent = e.Content;
        PageTitle = e.Title;
        Breadcrumb = BuildBreadcrumb(e.RouteKey, e.Title);

        // Active state is derived from the navigation that actually happened, so a route reached from
        // inside another screen still lands on the right pill (or clears them all, for a screen that is
        // not rail-visible).
        foreach (ShellRailItemViewModel item in _railItems)
        {
            item.IsActive = string.Equals(item.RouteKey, e.RouteKey, StringComparison.Ordinal);
        }
    }

    private string BuildBreadcrumb(string routeKey, string title)
    {
        ShellRailItemViewModel? railItem = Array.Find(
            _railItems,
            item => string.Equals(item.RouteKey, routeKey, StringComparison.Ordinal));

        return railItem is null ? title : railItem.GroupTitle + BreadcrumbSeparator + title;
    }
}
