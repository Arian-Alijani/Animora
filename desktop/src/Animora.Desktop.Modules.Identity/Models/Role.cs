namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// The role list row and the claim-assignment form's read model (SEC-09). Storage units only.
/// </summary>
/// <param name="Id">The role's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="DisplayName">Mirrors <c>IRoleInput.DisplayName</c>, e.g. <c>"پذیرش"</c>.</param>
/// <param name="PermissionClaimKeys">
/// Mirrors <c>IRoleInput.PermissionClaimKeys</c>, each a <see cref="PermissionClaim.Key"/> from
/// <see cref="PermissionCatalog"/>.
/// </param>
/// <param name="MemberCount">
/// How many staff members currently carry this as their primary role — what the role list shows so
/// an owner-admin can see the blast radius of a claim change before making it.
/// </param>
/// <param name="IsSystemRole">
/// Whether this is the tenant-seeded owner-admin role (SEC-11). The role-management screen uses
/// this flag to disable delete and to keep <c>staff.manage</c> checked and non-clearable for this
/// one row, rather than comparing against the owner-admin role's id from outside this flag.
/// </param>
public sealed record Role(
    Guid Id,
    string DisplayName,
    IReadOnlyCollection<string> PermissionClaimKeys,
    int MemberCount,
    bool IsSystemRole);
