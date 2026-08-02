using Animora.Desktop.Modules.Clients.Models;
using Animora.Desktop.Modules.Clients.ViewModels;
using Animora.Desktop.Modules.Clients.Views;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using FluentAssertions;

namespace Animora.Desktop.UiTests.Clients;

/// <summary>
/// One headless RTL smoke test per route this module registers (playbook step 5, PHASE.md
/// criteria 2-3), driven through <see cref="ShellRouteHarness"/> exactly like
/// <c>Identity/IdentityScreenSmokeTests</c>. The patient-list route gets two facts rather than one:
/// it is the single route serving both the global and the owner-scoped mode (the phase 05 TODO
/// header's decision), and a fact per mode is what keeps that decision from silently regressing
/// into a half-working second mode.
/// </summary>
public class ClientsScreenSmokeTests
{
    [AvaloniaFact]
    public void Owner_list_route_renders_OwnerListView_RightToLeft_and_loads_the_seeded_owners()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        OwnerListViewModel viewModel = Rendered<OwnerListView, OwnerListViewModel>(
            harness.NavigateTo(OwnerListViewModel.RouteKey));

        // Proof the screen is wired end to end: the ViewModel dispatched GetOwnerListQuery through
        // Mediator and ClientsSampleData's seeded rows came back, without the shell knowing any of it.
        viewModel.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public void Owner_form_route_renders_OwnerFormView_RightToLeft_in_create_mode_with_the_intake_date_pre_filled()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        OwnerFormViewModel viewModel = Rendered<OwnerFormView, OwnerFormViewModel>(
            harness.NavigateTo(OwnerFormViewModel.RouteKey));

        // No owner id was passed as the navigation parameter, so the form is in create mode
        // (OwnerFormViewModel.OnNavigatedTo), the mode OwnerListViewModel.CreateCommand reaches.
        viewModel.HeaderTitle.Should().Be("افزودن صاحب حیوان جدید");

        // "the form pre-fills today's date" (phase 05 TODO item 2's answer): a create that left the
        // date at default would still render, so the pre-fill needs its own assertion.
        viewModel.IntakeDate.Should().NotBe(default(DateTimeOffset));
    }

    [AvaloniaFact]
    public void Patient_list_route_renders_PatientListView_RightToLeft_unscoped_when_reached_from_the_rail()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        PatientListViewModel viewModel = Rendered<PatientListView, PatientListViewModel>(
            harness.NavigateTo(PatientListViewModel.RouteKey));

        viewModel.IsScoped.Should().BeFalse();
        viewModel.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public void Patient_list_route_scoped_to_an_owner_shows_that_owner_and_only_that_owner_s_patients()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        // The id comes from the owner list the user would have been looking at, not from the seeded
        // store, so this fact covers OwnerListViewModel.OpenPatientsCommand's actual hop.
        Owner owner = Rendered<OwnerListView, OwnerListViewModel>(
            harness.NavigateTo(OwnerListViewModel.RouteKey)).Items[0];

        PatientListViewModel viewModel = Rendered<PatientListView, PatientListViewModel>(
            harness.NavigateTo(PatientListViewModel.RouteKey, owner.Id));

        viewModel.IsScoped.Should().BeTrue();
        viewModel.ScopeOwnerName.Should().Be(owner.FullName);

        // An owner with no patients yet is a seeded case, so this stays a filter assertion rather
        // than a non-empty one.
        viewModel.Items.Should().OnlyContain(patient => patient.OwnerId == owner.Id);
    }

    [AvaloniaFact]
    public void Patient_form_route_renders_PatientFormView_RightToLeft_in_create_mode_with_no_pre_filled_owner()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        PatientFormViewModel viewModel = Rendered<PatientFormView, PatientFormViewModel>(
            harness.NavigateTo(PatientFormViewModel.RouteKey));

        viewModel.HeaderTitle.Should().Be("افزودن بیمار جدید");

        // The global-mode create carries no PatientFormNavigationParameter.OwnerId, so the owner
        // picker starts empty instead of locked (item 26's "or picked when absent" rule).
        viewModel.HasSelectedOwner.Should().BeFalse();
        viewModel.IsOwnerLocked.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Medical_file_summary_route_renders_MedicalFileSummaryView_RightToLeft_for_a_listed_patient()
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        Patient patient = Rendered<PatientListView, PatientListViewModel>(
            harness.NavigateTo(PatientListViewModel.RouteKey)).Items[0];

        MedicalFileSummaryViewModel viewModel = Rendered<MedicalFileSummaryView, MedicalFileSummaryViewModel>(
            harness.NavigateTo(MedicalFileSummaryViewModel.RouteKey, patient.Id));

        // The header read went through the patient seam and found the row the list just showed
        // (item 22); IsNotFound would also render, hence both assertions.
        viewModel.IsNotFound.Should().BeFalse();
        viewModel.PatientName.Should().Be(patient.Name);
        viewModel.OwnerDisplayName.Should().Be(patient.OwnerDisplayName);
    }

    // The two facts every route shares — right View, RTL inherited rather than re-set per screen
    // (DT-06, DESK-ARCH-06) — leaving each test above with only what is specific to its own screen.
    private static TViewModel Rendered<TView, TViewModel>(Control view)
        where TView : Control
        where TViewModel : class
    {
        view.Should().BeOfType<TView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);

        return view.DataContext.Should().BeOfType<TViewModel>().Which;
    }
}
