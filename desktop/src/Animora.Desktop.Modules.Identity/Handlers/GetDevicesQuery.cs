using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>The device-listing screen's one dispatch target: the read-only device rows (SEC-06).</summary>
public sealed record GetDevicesQuery : IQuery<IReadOnlyList<DeviceRegistration>>;
