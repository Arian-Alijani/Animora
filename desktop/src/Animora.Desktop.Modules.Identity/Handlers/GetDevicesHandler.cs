using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="GetDevicesQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetDevicesHandler : IQueryHandler<GetDevicesQuery, IReadOnlyList<DeviceRegistration>>
{
    private readonly IDeviceReadStore _readStore;

    public GetDevicesHandler(IDeviceReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<IReadOnlyList<DeviceRegistration>> Handle(GetDevicesQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<IReadOnlyList<DeviceRegistration>>(_readStore.GetAllAsync(cancellationToken));
    }
}
