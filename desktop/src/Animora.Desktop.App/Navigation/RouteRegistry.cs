using Animora.Desktop.UI.Navigation;

namespace Animora.Desktop.App.Navigation;

/// <summary>
/// The single <see cref="IRouteRegistry"/> implementation, held as a singleton by the composition
/// root. Registration is composition-time only and single-threaded (every
/// <c>Add&lt;Module&gt;Module()</c> call runs before the shell exists), so the collections are
/// deliberately unsynchronized; reads afterwards are effectively immutable.
/// </summary>
public sealed class RouteRegistry : IRouteRegistry
{
    // Ordinal: route keys are kebab-case identifiers (CONV-12), not user text, so a casing
    // difference is a typo to surface rather than a match to make.
    private readonly Dictionary<string, RouteDescriptor> _routes = new(StringComparer.Ordinal);
    private readonly List<NavigationEntry> _railEntries = [];

    private NavigationEntry[]? _orderedRailEntries;

    /// <inheritdoc />
    public IReadOnlyList<NavigationEntry> RailEntries =>
        // OrderBy is a stable sort, so registration order is the documented tie-break for equal
        // RailOrder values without carrying an explicit sequence number.
        _orderedRailEntries ??= [.. _railEntries.OrderBy(entry => entry.RailGroup).ThenBy(entry => entry.RailOrder)];

    /// <inheritdoc />
    public void Register(RouteDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!_routes.TryAdd(descriptor.RouteKey, descriptor))
        {
            throw new InvalidOperationException(
                $"Route key '{descriptor.RouteKey}' is already registered. Overwriting would let one module shadow another module's screen (DT-09).");
        }

        if (descriptor.ToRailEntry() is { } entry)
        {
            _railEntries.Add(entry);
            _orderedRailEntries = null;
        }
    }

    /// <summary>
    /// Route lookup for <see cref="NavigationService"/>. Kept off <see cref="IRouteRegistry"/> so a
    /// module cannot bypass <see cref="INavigationService"/> and construct another module's screen
    /// from its descriptor.
    /// </summary>
    /// <exception cref="KeyNotFoundException">The key was never registered.</exception>
    public RouteDescriptor GetRequired(string routeKey)
    {
        if (!_routes.TryGetValue(routeKey, out RouteDescriptor? descriptor))
        {
            throw new KeyNotFoundException(
                $"No route is registered for key '{routeKey}'. Registered keys: {string.Join(", ", _routes.Keys)}.");
        }

        return descriptor;
    }
}
