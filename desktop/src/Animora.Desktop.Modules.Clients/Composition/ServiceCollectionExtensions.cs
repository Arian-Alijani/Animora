using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.ViewModels;
using Animora.Desktop.Modules.Clients.Views;
using Animora.Desktop.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Animora.Desktop.Modules.Clients.Composition;

/// <summary>
/// This module's one registration surface (DIR-03, DT-09), copying the shape
/// <c>Modules.Identity</c>'s own extension set (item 28): the composition root calls
/// <see cref="AddClientsModule"/> and gets the module's five routes, their view models and the
/// Stage A data-seam bindings together — routes, screens, data seams, in that order — so phase 16's
/// Stage C rebind stays one line each.
/// </summary>
public static class ServiceCollectionExtensions
{
    // Owner/patient management sits in the same rail group as Identity's staff/role/device screens
    // (RailGroup only has three members observed on the reference sidebar — design-reference.md §6
    // — and Clients has no group of its own to pick from): both are facets of "managing this
    // clinic's records", the same phase-04 default Modules.Identity's own composition extension
    // already records for its own five routes, continuing that group's rail order (0-3) at 4-5.
    private const string OwnerListRouteTitle = "صاحبان حیوانات";
    private const string OwnerFormRouteTitle = "فرم صاحب حیوان";
    private const string PatientListRouteTitle = "بیماران";
    private const string PatientFormRouteTitle = "فرم بیمار";
    private const string MedicalFileSummaryRouteTitle = "پرونده پزشکی";

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
    public static IServiceCollection AddClientsModule(this IServiceCollection services, IRouteRegistry routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        routes.Register(RouteDescriptor.Create<OwnerListViewModel, OwnerListView>(
            OwnerListViewModel.RouteKey,
            OwnerListRouteTitle,
            iconGlyph: "mdi-account-multiple",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 4));

        // Not rail-visible: reached only from OwnerListViewModel's create/edit commands (DESK-ARCH-05).
        routes.Register(RouteDescriptor.Create<OwnerFormViewModel, OwnerFormView>(
            OwnerFormViewModel.RouteKey,
            OwnerFormRouteTitle,
            iconGlyph: "mdi-account-edit"));

        routes.Register(RouteDescriptor.Create<PatientListViewModel, PatientListView>(
            PatientListViewModel.RouteKey,
            PatientListRouteTitle,
            iconGlyph: "mdi-paw",
            railGroup: RailGroup.ClinicManagement,
            railOrder: 5));

        // Not rail-visible: reached from PatientListViewModel's create/edit commands and
        // OwnerListViewModel.OpenPatientsCommand's owner-scoped create (DESK-ARCH-05).
        routes.Register(RouteDescriptor.Create<PatientFormViewModel, PatientFormView>(
            PatientFormViewModel.RouteKey,
            PatientFormRouteTitle,
            iconGlyph: "mdi-paw-outline"));

        // Not rail-visible: reached only from PatientListViewModel.OpenMedicalFileCommand (item 27).
        routes.Register(RouteDescriptor.Create<MedicalFileSummaryViewModel, MedicalFileSummaryView>(
            MedicalFileSummaryViewModel.RouteKey,
            MedicalFileSummaryRouteTitle,
            iconGlyph: "mdi-file-document-outline"));

        // Transient: NavigationService builds a fresh view model per navigation so each screen's
        // load request re-runs on every visit, mirroring Modules.Identity's own registrations.
        services.TryAddTransient<OwnerListViewModel>();
        services.TryAddTransient<OwnerFormViewModel>();
        services.TryAddTransient<PatientListViewModel>();
        services.TryAddTransient<PatientFormViewModel>();
        services.TryAddTransient<MedicalFileSummaryViewModel>();

        // TODO(P1-16): rebind every seam below to the Dapper-backed readers and EF Core-backed
        // writers over the local database (DT-05, INV-20); nothing but these registrations changes.
        // Singleton because AddMediator registers handlers as singletons, and a scoped/transient
        // dependency of a singleton handler would be a captive dependency — the same reasoning
        // Modules.Identity's own Stage A binding already carries.
        services.TryAddSingleton<ClientsSampleData>();

        // One shared instance bound under both halves (DIR-03, DT-05): a create through the write
        // half must show up in the read half's next query the way one SQLite table would, exactly
        // what InMemoryOwnerStore/InMemoryPatientStore's own doc comments describe.
        services.TryAddSingleton<InMemoryOwnerStore>();
        services.TryAddSingleton<IOwnerReadStore>(static provider => provider.GetRequiredService<InMemoryOwnerStore>());
        services.TryAddSingleton<IOwnerWriteStore>(static provider => provider.GetRequiredService<InMemoryOwnerStore>());

        services.TryAddSingleton<InMemoryPatientStore>();
        services.TryAddSingleton<IPatientReadStore>(static provider => provider.GetRequiredService<InMemoryPatientStore>());
        services.TryAddSingleton<IPatientWriteStore>(static provider => provider.GetRequiredService<InMemoryPatientStore>());

        return services;
    }
}
