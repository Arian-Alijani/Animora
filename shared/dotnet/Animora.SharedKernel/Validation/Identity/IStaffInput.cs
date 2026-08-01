namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// The property surface a staff (user) create/edit command implements, so
/// <see cref="StaffValidator"/> runs directly against the command (CONV-18, INV-02) instead of a
/// copied input DTO.
/// </summary>
/// <remarks>
/// Only the fields the validator judges are listed: a staff member's activation flag, avatar or
/// audit stamps travel on the command without appearing here, because a property no rule reads
/// would only invite a rule to be written in the wrong layer.
/// </remarks>
public interface IStaffInput
{
    /// <summary>The staff member's full name, as the staff list and the top-bar chip display it.</summary>
    string FullName { get; }

    /// <summary>
    /// The sign-in identifier (SEC-03). Lower-case ASCII so that "Sara" and "sara" can never become
    /// two accounts on a case-sensitive store, and so it stays typable on a Persian keyboard layout.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Iranian mobile number, e.g. <c>"09121234567"</c> — required, since staff-facing alerts and
    /// the account-recovery channel both go out on it (see 14-jobs-and-notifications.md).
    /// </summary>
    string MobileNumber { get; }

    /// <summary>Optional e-mail address; not every clinic issues one to every staff member.</summary>
    string? Email { get; }

    /// <summary>
    /// The one primary role this staff member is assigned (SEC-10). Supplementary per-user grants
    /// are a separate surface and are not part of the create/edit form.
    /// </summary>
    Guid RoleId { get; }
}
