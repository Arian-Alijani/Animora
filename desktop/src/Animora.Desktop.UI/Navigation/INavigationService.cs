namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// The single way any ViewModel changes screen (DESK-ARCH-05): it names a route key a module
/// published through <see cref="IRouteRegistry"/> and never the target's View or ViewModel type, so
/// one module can send the user to another module's screen without referencing it (DT-01).
/// <para>
/// Implementations build Avalonia controls, so every member is UI-thread affine; the shell subscribes
/// to <see cref="RouteChanged"/> rather than polling <see cref="CurrentRouteKey"/>.
/// </para>
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Key of the route currently shown, or <see langword="null"/> before the first navigation. Lets
    /// the rail mark its active item without keeping a second copy of navigation state.
    /// </summary>
    string? CurrentRouteKey { get; }

    /// <summary>
    /// Raised after the target screen is built and before it is displayed, carrying everything the
    /// shell needs to swap its content region (DESK-ARCH-01: the shell reacts, it never constructs a
    /// module screen itself).
    /// </summary>
    event EventHandler<RouteChangedEventArgs>? RouteChanged;

    /// <summary>
    /// Navigates to <paramref name="routeKey"/>, optionally handing <paramref name="parameter"/> to a
    /// target ViewModel that implements <see cref="INavigationAware"/> (e.g. the record id a list
    /// screen passes to a detail screen). An unregistered key fails loudly rather than silently
    /// leaving the current screen in place.
    /// </summary>
    void NavigateTo(string routeKey, object? parameter = null);
}
