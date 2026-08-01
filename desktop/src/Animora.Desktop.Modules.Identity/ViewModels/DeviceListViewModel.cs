using System.Collections.ObjectModel;
using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.Modules.Identity.Models;
using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.Input;
using Mediator;

namespace Animora.Desktop.Modules.Identity.ViewModels;

/// <summary>
/// The device-listing screen (SEC-06, LIC-08): a read-only grid over <see cref="GetDevicesQuery"/>
/// (item 25). No revoke action and no seat/limit UI here — both are server-side (LIC-08/LIC-09) and
/// out of scope for this phase's screens (PHASE.md scope).
/// </summary>
public sealed class DeviceListViewModel : ViewModelBase, INavigationAware
{
    /// <summary>Rail-visible navigation key this screen registers under (item 31 wires it).</summary>
    public const string RouteKey = "device-list";

    private readonly IMediator _mediator;

    private bool _isLoading;

    public DeviceListViewModel(IMediator mediator)
    {
        _mediator = mediator;

        // Hand-built rather than [RelayCommand]: the generator is an analyzer asset this project
        // only sees transitively (same reason as the module's other screens).
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public IAsyncRelayCommand LoadCommand { get; }

    /// <summary>
    /// Unpaged, mirroring <see cref="Data.IDeviceReadStore.GetAllAsync"/>'s own reasoning: a
    /// tenant's device count is bounded by its license seat limit, far short of DT-08's threshold.
    /// </summary>
    public ObservableCollection<DeviceRegistration> Items { get; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <inheritdoc />
    public void OnNavigatedTo(object? parameter)
    {
        // Started, never awaited: navigation must not block on a read (DESK-ARCH-10).
        LoadCommand.Execute(null);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            var devices = await _mediator.Send(new GetDevicesQuery(), cancellationToken).ConfigureAwait(true);

            Items.Clear();
            foreach (var device in devices)
            {
                Items.Add(device);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
