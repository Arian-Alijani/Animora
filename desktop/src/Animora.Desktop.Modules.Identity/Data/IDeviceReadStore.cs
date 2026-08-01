using Animora.Desktop.Modules.Identity.Models;

namespace Animora.Desktop.Modules.Identity.Data;

/// <summary>
/// The read seam behind the device-listing screen (SEC-06), declared by the module that consumes it
/// (DIR-03 applied to the desktop): Stage A composition binds an in-memory fake, Stage C rebinds a
/// Dapper reader (DT-05, INV-20).
/// </summary>
/// <remarks>
/// No write half: the listing screen is read-only in this phase (PHASE.md scope) — device
/// registration and revocation both happen server-side (LIC-08/LIC-09), so there is nothing for the
/// desktop to write here, unlike <see cref="IStaffWriteStore"/> or <see cref="IRoleWriteStore"/>.
/// </remarks>
public interface IDeviceReadStore
{
    /// <summary>
    /// Reads every device registered to the tenant, ordered by
    /// <see cref="DeviceRegistration.LastActiveAtUtc"/> descending so the most recently used devices
    /// surface first.
    /// </summary>
    /// <remarks>
    /// Unpaged on purpose, mirroring <see cref="IRoleReadStore.GetAllAsync"/>'s reasoning: a tenant's
    /// device count is bounded by its license seat limit (LIC-08), far short of DT-08's 200-row
    /// virtualization threshold.
    /// </remarks>
    Task<IReadOnlyList<DeviceRegistration>> GetAllAsync(CancellationToken cancellationToken);
}
