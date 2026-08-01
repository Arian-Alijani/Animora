using Animora.Desktop.Modules.Reporting.Data;
using Animora.Desktop.Modules.Reporting.ViewModels;
using Animora.Desktop.Modules.Reporting.Views;
using Animora.Desktop.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Animora.Desktop.Modules.Reporting.Composition;

/// <summary>
/// This module's one registration surface (DIR-03, DT-09): the composition root calls
/// <see cref="AddReportingModule"/> and gets the module's routes, screens, handler dependencies and
/// Stage A data bindings together. Every module UI phase copies this shape, so the three kinds of
/// registration stay in this order — routes, screens, data seams — and the Stage A binding stays one
/// line for phase 21 to rebind.
/// <para>
/// <paramref name="routes"/> is passed in rather than resolved: registration happens before the
/// container is built, and the shell must read back the same registry instance the modules wrote to.
/// </para>
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Navigation key of this module's landing screen, kebab-case per CONV-12.</summary>
    public const string HomeRouteKey = "home";

    // The module's own chrome label. It sits here, not in the view, because the rail and the shell's
    // page title read it from the descriptor; the screen's own labels stay in HomeView.axaml (AG-11).
    private const string HomeRouteTitle = "خانه";

    public static IServiceCollection AddReportingModule(this IServiceCollection services, IRouteRegistry routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // First rail group, first item: this is the route ShellViewModel lands on at startup.
        routes.Register(RouteDescriptor.Create<HomeViewModel, HomeView>(
            HomeRouteKey,
            HomeRouteTitle,
            iconGlyph: "mdi-home",
            railGroup: RailGroup.CommandCenter,
            railOrder: 0));

        // Transient: NavigationService builds a fresh view model per navigation so the screen's load
        // request re-runs on every visit.
        services.TryAddTransient<HomeViewModel>();

        // TODO(P1-21): rebind to the Dapper-backed read store. Singleton because AddMediator registers
        // handlers as singletons, and a scoped/transient dependency of a singleton handler would be a
        // captive dependency.
        services.TryAddSingleton<IHomeSummaryReadStore, InMemoryHomeSummaryReadStore>();

        return services;
    }
}
