using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.ViewModels;
using Animora.Desktop.Modules.Identity.Views;
using Animora.Desktop.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Animora.Desktop.Modules.Identity.Composition;

/// <summary>
/// This module's one registration surface (DIR-03, DT-09), copying the shape
/// <c>Modules.Reporting</c>'s own extension set (item 5): the composition root calls
/// <see cref="AddIdentityModule"/> and gets the module's five routes, their view models and the
/// Stage A data-seam bindings together — routes, screens, data seams, in that order — so the Stage A
/// binding stays one line each for phase 15 to rebind.
/// </summary>
public static class ServiceCollectionExtensions
{
    // Login and the device list sit beside staff/role management in the same rail group: every
    // screen this module owns is a facet of "who can use this clinic account", not a distinct
    // product area of its own — a phase-04 default recorded here the way item 28's username-field
    // split is (AG-02), open to correction once a later phase's rail has more groups to compare
    // against.
    private const string LoginRouteTitle = "ورود";
    private const string StaffListRouteTitle = "کارکنان";
    private const string StaffFormRouteTitle = "فرم کارمند";
    private const string RoleManagementRouteTitle = "نقش‌ها و دسترسی‌ها";
    private const string DeviceListRouteTitle = "دستگاه‌های ثبت‌شده";

    /// <summary>
    /// Registers this module's routes, screens and Stage A data seams in one call, so the
    /// composition root never names a screen, a view model or a data store of its own (DESK-ARCH-05).
    /// </summary>
    /// <param name="services">The container being described by the composition root.</param>
    /// <param name="routes">
    /// The shell's route registry, passed in rather than resolved: registration happens before the
    /// container is built, and the shell must read back the same registry instance the modules wrote to.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance, so registrations can be chained.</returns>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IRouteRegistry routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        // Rail-visible: the login screen carries its own TODO(P2) marker (LoginView.axaml) recording
        // that real authentication moves it ahead of the shell once a server exists.
        routes.Register(RouteDescriptor.Create<LoginViewModel, LoginView>(
            LoginViewModel.RouteKey,
            LoginRouteTitle,
            iconGlyph: "mdi-login-variant",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 0));

        routes.Register(RouteDescriptor.Create<StaffListViewModel, StaffListView>(
            StaffListViewModel.RouteKey,
            StaffListRouteTitle,
            iconGlyph: "mdi-account-group",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 1));

        // Not rail-visible: reached only from StaffListViewModel's create/edit commands (DESK-ARCH-05).
        routes.Register(RouteDescriptor.Create<StaffFormViewModel, StaffFormView>(
            StaffFormViewModel.RouteKey,
            StaffFormRouteTitle,
            iconGlyph: "mdi-account-edit"));

        routes.Register(RouteDescriptor.Create<RoleManagementViewModel, RoleManagementView>(
            RoleManagementViewModel.RouteKey,
            RoleManagementRouteTitle,
            iconGlyph: "mdi-shield-account",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 2));

        routes.Register(RouteDescriptor.Create<DeviceListViewModel, DeviceListView>(
            DeviceListViewModel.RouteKey,
            DeviceListRouteTitle,
            iconGlyph: "mdi-cellphone-link",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 3));

        // Transient: NavigationService builds a fresh view model per navigation so each screen's
        // load request re-runs on every visit, mirroring Modules.Reporting's HomeViewModel.
        services.TryAddTransient<LoginViewModel>();
        services.TryAddTransient<StaffListViewModel>();
        services.TryAddTransient<StaffFormViewModel>();
        services.TryAddTransient<RoleManagementViewModel>();
        services.TryAddTransient<DeviceListViewModel>();

        // TODO(P1-15): rebind every seam below to the Dapper-backed readers and EF Core-backed
        // writers over the local database (DT-05, INV-20); nothing but these registrations changes.
        // Singleton because AddMediator registers handlers as singletons, and a scoped/transient
        // dependency of a singleton handler would be a captive dependency — the same reasoning
        // Modules.Reporting's own Stage A binding already carries.
        services.TryAddSingleton<IdentitySampleData>();

        // One shared instance bound under both halves (DIR-03, DT-05): a create through the write
        // half must show up in the read half's next query the way one SQLite table would, which is
        // exactly what InMemoryStaffStore/InMemoryRoleStore's own doc comments describe.
        services.TryAddSingleton<InMemoryStaffStore>();
        services.TryAddSingleton<IStaffReadStore>(static provider => provider.GetRequiredService<InMemoryStaffStore>());
        services.TryAddSingleton<IStaffWriteStore>(static provider => provider.GetRequiredService<InMemoryStaffStore>());

        services.TryAddSingleton<InMemoryRoleStore>();
        services.TryAddSingleton<IRoleReadStore>(static provider => provider.GetRequiredService<InMemoryRoleStore>());
        services.TryAddSingleton<IRoleWriteStore>(static provider => provider.GetRequiredService<InMemoryRoleStore>());

        services.TryAddSingleton<IDeviceReadStore, InMemoryDeviceReadStore>();

        // TODO(P2): rebind to the server sign-in endpoint instead (DT-12, SEC-01, SEC-03) — see
        // IStaffCredentialReadStore's own TODO(P2).
        services.TryAddSingleton<IStaffCredentialReadStore, InMemoryStaffCredentialReadStore>();

        return services;
    }
}
