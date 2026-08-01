using Animora.Desktop.Modules.Identity.Models;

namespace Animora.Desktop.Modules.Identity.Data;

// TODO(P1-15): delete this type and rebind IDeviceReadStore to the Dapper-backed reader over the
// local database (DT-05, INV-20). Nothing but one registration line in
// Composition/ServiceCollectionExtensions changes with it (DIR-03).
/// <summary>Satisfies <see cref="IDeviceReadStore"/> over <see cref="IdentitySampleData"/>.</summary>
internal sealed class InMemoryDeviceReadStore : IDeviceReadStore
{
    private readonly IdentitySampleData _sampleData;

    public InMemoryDeviceReadStore(IdentitySampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<IReadOnlyList<DeviceRegistration>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            IReadOnlyList<DeviceRegistration> devices = _sampleData.Devices
                .OrderByDescending(device => device.LastActiveAtUtc)
                .ToList();

            return Task.FromResult(devices);
        }
    }
}
