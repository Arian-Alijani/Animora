using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using Avalonia.Controls;

namespace Animora.Desktop.App.Navigation;

/// <summary>
/// The single <see cref="INavigationService"/> implementation. It reaches a module screen only through
/// the descriptor that module registered — resolving the ViewModel from the container and letting the
/// descriptor build the View — so this project keeps zero compile-time knowledge of any module screen
/// (DESK-ARCH-05, DT-09).
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly RouteRegistry _registry;
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(RouteRegistry registry, IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public string? CurrentRouteKey { get; private set; }

    /// <inheritdoc />
    public event EventHandler<RouteChangedEventArgs>? RouteChanged;

    /// <inheritdoc />
    public void NavigateTo(string routeKey, object? parameter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeKey);

        RouteDescriptor descriptor = _registry.GetRequired(routeKey);

        // Screens are rebuilt on every navigation, including a repeat of the current route: a fresh
        // ViewModel re-runs the screen's load request, which is what makes returning to a list show
        // rows written since it was left. Caching would need per-screen invalidation rules that no
        // module has asked for.
        ViewModelBase viewModel = descriptor.CreateViewModel(_serviceProvider);

        if (viewModel is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedTo(parameter);
        }

        Control view = descriptor.CreateView(viewModel);

        CurrentRouteKey = routeKey;
        RouteChanged?.Invoke(this, new RouteChangedEventArgs(routeKey, descriptor.Title, view));
    }
}
