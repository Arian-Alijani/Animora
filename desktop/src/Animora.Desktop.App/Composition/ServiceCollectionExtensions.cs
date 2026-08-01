using Animora.Desktop.App.AppState;
using Animora.Desktop.App.Navigation;
using Animora.Desktop.App.Shell;
using Animora.Desktop.Modules.Reporting.Composition;
using Animora.Desktop.UI.AppState;
using Animora.Desktop.UI.Navigation;
using Animora.Desktop.UI.Services;
using Avalonia.Controls.Notifications;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Animora.Desktop.App.Composition;

/// <summary>
/// The composition root's single registration surface (DIR-07): <c>Startup/StartupSequence</c> calls
/// <see cref="AddDesktopApp"/> once during host bootstrap and every app-wide service exists
/// afterwards. Module registrations are appended here as their phases land — this is the one place
/// allowed to name a module (DESK-ARCH-05), which is what keeps <c>Shell/</c> and <c>Navigation/</c>
/// free of any module type.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApp(this IServiceCollection services)
    {
        // The design system registers itself (icons, formatters, dialog/toast mappings); this root
        // never wires those services one by one.
        services.AddDesktopUi();

        // Generated registration: it discovers the handlers of every referenced module assembly
        // (TECH_STACK §4), so a new module's handlers arrive with its project reference. Singleton is
        // Mediator's recommended lifetime and the right one here — a desktop process has no
        // per-request scope for handlers to align with, and handlers hold no state (DESK-ARCH-02).
        services.AddMediator((MediatorOptions options) => options.ServiceLifetime = ServiceLifetime.Singleton);

        // Constructed here instead of resolved: the module registrations below publish their routes
        // while the container is still being described, and the shell has to read back the same
        // instance they wrote to. One instance behind two entries — modules see IRouteRegistry, while
        // NavigationService needs the concrete type's route lookup, which is deliberately not on the
        // interface (RouteRegistry.GetRequired).
        RouteRegistry routes = new();
        services.TryAddSingleton(routes);
        services.TryAddSingleton<IRouteRegistry>(routes);
        services.TryAddSingleton<INavigationService, NavigationService>();

        // Injected singletons rather than state on ViewModelBase (DESK-ARCH-03); both are phase-02
        // placeholders carrying their own swap markers.
        services.TryAddSingleton<ICurrentUserState, CurrentUserState>();
        services.TryAddSingleton<IAppStatusState, AppStatusState>();

        // The shell view model is a singleton because it *is* the window's state: the startup sequence
        // navigates the landing route through the same instance the window binds to.
        services.TryAddSingleton<ShellViewModel>();
        services.TryAddSingleton<ShellWindow>();

        // ToastService depends on the manager abstraction, but the only implementation is window-bound,
        // and the window does not exist until the startup sequence's shell stage — hence a factory
        // resolved from ShellWindow instead of an instance registered here.
        services.TryAddSingleton<INotificationManager>(static provider =>
            new WindowNotificationManager(provider.GetRequiredService<ShellWindow>())
            {
                // Opposite corner from the top bar so a toast never covers the persistent status
                // indicator (DESK-ARCH-07); the physical side follows the inherited flow direction.
                Position = NotificationPosition.BottomLeft,
            });

        // The module's only appearance in this project, and the proof of DESK-ARCH-05: the shell and
        // navigation code above name no screen, no view model and no read store — this line is what
        // makes the Home route exist. Later phases append one call each here (DT-09).
        services.AddReportingModule(routes);

        return services;
    }
}
