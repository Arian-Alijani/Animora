using Animora.Desktop.Modules.Identity.ViewModels;
using Animora.Desktop.Modules.Identity.Views;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using FluentAssertions;

namespace Animora.Desktop.UiTests.Identity;

/// <summary>
/// One headless RTL smoke test per route this module registers (playbook step 5, PHASE.md
/// criterion 2), driven through <see cref="ShellRouteHarness"/>: the real composition root is
/// built, the shell is shown at its default route, and navigation — never a View or ViewModel
/// constructed by hand — takes it to the route under test.
/// </summary>
public class IdentityScreenSmokeTests
{
    [AvaloniaFact]
    public void Login_route_renders_LoginView_RightToLeft()
    {
        Control view = NavigateAndGetContent(LoginViewModel.RouteKey);

        view.Should().BeOfType<LoginView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);
        view.DataContext.Should().BeOfType<LoginViewModel>();
    }

    [AvaloniaFact]
    public void Staff_list_route_renders_StaffListView_RightToLeft_and_loads_the_seeded_staff()
    {
        Control view = NavigateAndGetContent(StaffListViewModel.RouteKey);

        view.Should().BeOfType<StaffListView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);

        // Proof the screen is wired end to end: the ViewModel dispatched GetStaffListQuery through
        // Mediator and IdentitySampleData's seeded rows came back, without the shell knowing any of it.
        view.DataContext.Should().BeOfType<StaffListViewModel>()
            .Which.Items.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public void Staff_form_route_renders_StaffFormView_RightToLeft_in_create_mode_with_roles_loaded()
    {
        Control view = NavigateAndGetContent(StaffFormViewModel.RouteKey);

        view.Should().BeOfType<StaffFormView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);

        StaffFormViewModel viewModel = view.DataContext.Should().BeOfType<StaffFormViewModel>().Which;
        viewModel.Roles.Should().NotBeEmpty();
        // No staff id was passed as the navigation parameter, so the form is in create mode
        // (StaffFormViewModel.OnNavigatedTo), the mode StaffListViewModel.CreateCommand reaches.
        viewModel.HeaderTitle.Should().Be("افزودن کارمند جدید");
    }

    [AvaloniaFact]
    public void Role_management_route_renders_RoleManagementView_RightToLeft_with_a_role_selected()
    {
        Control view = NavigateAndGetContent(RoleManagementViewModel.RouteKey);

        view.Should().BeOfType<RoleManagementView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);

        RoleManagementViewModel viewModel = view.DataContext.Should().BeOfType<RoleManagementViewModel>().Which;
        // The tenant's system-seeded owner-admin role is one of the seeded rows (SEC-11), whichever
        // row LoadAsync's first-page default happens to land on.
        viewModel.Roles.Should().Contain(role => role.IsSystemRole);
        viewModel.SelectedRole.Should().NotBeNull();
        viewModel.ClaimGroups.Should().NotBeEmpty();
    }

    [AvaloniaFact]
    public void Device_list_route_renders_DeviceListView_RightToLeft_and_loads_the_seeded_devices()
    {
        Control view = NavigateAndGetContent(DeviceListViewModel.RouteKey);

        view.Should().BeOfType<DeviceListView>();
        view.FlowDirection.Should().Be(FlowDirection.RightToLeft);
        view.DataContext.Should().BeOfType<DeviceListViewModel>()
            .Which.Items.Should().NotBeEmpty();
    }

    private static Control NavigateAndGetContent(string routeKey)
    {
        using ShellRouteHarness harness = ShellRouteHarness.Start();

        return harness.NavigateTo(routeKey);
    }
}
