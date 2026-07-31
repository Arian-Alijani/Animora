namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// The surface a module's composition extension calls to publish its screens (DT-09): the module
/// hands over descriptors, the shell reads back rail entries, and neither side names a type from the
/// other (DIR-07). Registration happens once during composition, before the shell is shown, so the
/// implementation is not expected to support concurrent registration.
/// </summary>
public interface IRouteRegistry
{
    /// <summary>
    /// Rail-visible routes ordered by <see cref="RailGroup"/> then <see cref="NavigationEntry.RailOrder"/>,
    /// with registration order as the tie-break — so the rail is identical on every launch regardless
    /// of the order modules were registered in.
    /// </summary>
    IReadOnlyList<NavigationEntry> RailEntries { get; }

    /// <summary>
    /// Publishes one route. Implementations reject a duplicate
    /// <see cref="RouteDescriptor.RouteKey"/> rather than overwriting, so a module can never shadow
    /// another module's screen.
    /// </summary>
    void Register(RouteDescriptor descriptor);
}
