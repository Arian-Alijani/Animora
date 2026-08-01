namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// The staff list row and the staff create/edit form's read model. Storage units only — Persian
/// digit formatting is applied at the binding edge, never here (INV-13, DESK-ARCH-14).
/// </summary>
/// <param name="Id">The staff member's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="FullName">Mirrors <c>IStaffInput.FullName</c>.</param>
/// <param name="Username">Mirrors <c>IStaffInput.Username</c> (SEC-03).</param>
/// <param name="MobileNumber">Mirrors <c>IStaffInput.MobileNumber</c>.</param>
/// <param name="Email">Mirrors <c>IStaffInput.Email</c>.</param>
/// <param name="RoleId">Mirrors <c>IStaffInput.RoleId</c>, the one primary role (SEC-10).</param>
/// <param name="RoleDisplayName">
/// The assigned role's <see cref="Role.DisplayName"/> at read time. A read-side denormalization,
/// not a cross-aggregate embed: this shape is the module's own hot-read projection (Stage C
/// resolves it with one joined Dapper query, DT-05), so carrying the name here is what spares the
/// virtualized staff list a per-row round trip it would otherwise need (DT-08).
/// </param>
/// <param name="IsActive">
/// Whether the account can sign in. Not judged by <c>StaffValidator</c> — activation is a status
/// change, not an input-shape rule — and is exactly what <c>SignInHandler</c> checks to distinguish
/// <c>IdentityErrors.AccountInactive</c> from a successful sign-in.
/// </param>
public sealed record StaffMember(
    Guid Id,
    string FullName,
    string Username,
    string MobileNumber,
    string? Email,
    Guid RoleId,
    string RoleDisplayName,
    bool IsActive);
