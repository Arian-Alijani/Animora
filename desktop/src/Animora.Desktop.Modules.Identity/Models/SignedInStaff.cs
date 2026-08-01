namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// What a successful sign-in yields for the session projection (DT-12, SEC-07): identity, role and
/// claim data for UI-level display and gating only.
/// </summary>
/// <remarks>
/// No token, no refresh material and nothing else a real authentication flow would issue: those
/// stay server-side seams until P2 (DT-12), and the desktop never persists this shape beyond the
/// in-memory session — SignInHandler's caller projects it onto <c>ICurrentUserState</c> and nothing
/// else holds a copy.
/// </remarks>
/// <param name="StaffId">The signed-in <see cref="StaffMember.Id"/>.</param>
/// <param name="FullName">For <c>ICurrentUserState.DisplayName</c> and its avatar initial.</param>
/// <param name="RoleId">The signed-in staff member's primary <see cref="Role.Id"/> (SEC-10).</param>
/// <param name="RoleDisplayName">For <c>ICurrentUserState.RoleDisplayName</c>.</param>
/// <param name="PermissionClaimKeys">
/// The role's <see cref="Role.PermissionClaimKeys"/> at sign-in time, carried here so a screen can
/// gate a command or a rail entry at the UI level (PHASE.md scope) without a second round trip
/// through the Identity module for every check.
/// </param>
public sealed record SignedInStaff(
    Guid StaffId,
    string FullName,
    Guid RoleId,
    string RoleDisplayName,
    IReadOnlyCollection<string> PermissionClaimKeys);
