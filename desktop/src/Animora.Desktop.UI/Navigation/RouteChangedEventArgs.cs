using Avalonia.Controls;

namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// The result of one navigation: the route that is now active plus its already-built View. The View
/// travels on the event because <see cref="RouteDescriptor"/> owns its construction — the shell only
/// hosts what it is handed (DESK-ARCH-05).
/// </summary>
public sealed class RouteChangedEventArgs : EventArgs
{
    public RouteChangedEventArgs(string routeKey, string title, Control content)
    {
        RouteKey = routeKey;
        Title = title;
        Content = content;
    }

    /// <summary>Key of the now-active route; the rail matches its active item against this.</summary>
    public string RouteKey { get; }

    /// <summary>The route's Persian title, for the shell's page title and breadcrumb.</summary>
    public string Title { get; }

    /// <summary>The built View to place in the shell's content region.</summary>
    public Control Content { get; }
}
