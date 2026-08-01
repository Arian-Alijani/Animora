using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// The role-management screen's roles read (SEC-09). The claim catalog half of that screen's data
/// is <see cref="PermissionCatalog.All"/> — a compiled-in transcription, not a data-seam read — so it
/// is not part of this query's response (INV-18: one source per fact).
/// </summary>
public sealed record GetRolesQuery : IQuery<IReadOnlyList<Role>>;
