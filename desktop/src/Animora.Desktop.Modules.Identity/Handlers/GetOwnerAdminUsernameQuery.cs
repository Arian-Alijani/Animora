using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// The staff form's SEC-17 anchor lookup: resolves the tenant's current owner-admin username so a
/// create for any other role can render it as a fixed username prefix (item 28's UI rule), mirroring
/// the exact value <see cref="SaveStaffMemberHandler"/> checks the submitted username against at
/// save time. Returns <see langword="null"/> when no staff member holds the role yet.
/// </summary>
public sealed record GetOwnerAdminUsernameQuery : IQuery<string?>;
