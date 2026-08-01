namespace Animora.Contracts.Errors;

/// <summary>
/// Stable <c>ERR-IDENTITY-{NNN}</c> codes the Identity module's handlers return (CONV-13/14/15).
/// </summary>
/// <remarks>
/// Append-only and monotonically numbered (SH-03): a code is never renumbered or reused after
/// retirement, and this phase's handlers (see
/// <c>Roadmap/Desktop/phases/04-identity-auth-screens/TODO.md</c> items 19-24) are expected to be
/// the only source of new entries for a while — a later phase adds to this list rather than
/// changing what a shipped number means.
/// </remarks>
public static class IdentityErrors
{
    /// <summary>
    /// Sign-in submitted a username that does not resolve to an account, or the wrong password for
    /// one that does (SEC-03).
    /// </summary>
    /// <remarks>
    /// One code for both cases on purpose: a distinct "unknown user" code would let the login form
    /// be used to enumerate valid usernames, which is exactly the leak a single generic code closes.
    /// </remarks>
    public const string InvalidCredentials = "ERR-IDENTITY-001";

    /// <summary>Sign-in matched a real account, but the staff member is deactivated.</summary>
    public const string AccountInactive = "ERR-IDENTITY-002";

    /// <summary>
    /// A command's shared <c>FluentValidation</c> validator (CONV-18) rejected the submitted input.
    /// </summary>
    public const string ValidationFailed = "ERR-IDENTITY-003";

    /// <summary>
    /// A staff create/edit submitted a <c>Username</c> already held by another account in the
    /// tenant — the uniqueness check <c>StaffValidator</c> deliberately leaves to the handler.
    /// </summary>
    public const string UsernameAlreadyTaken = "ERR-IDENTITY-004";

    /// <summary>
    /// A staff create/edit referenced a <c>RoleId</c> that does not exist — the existence check
    /// <c>StaffValidator</c> deliberately leaves to the handler.
    /// </summary>
    public const string RoleNotFound = "ERR-IDENTITY-005";

    /// <summary>
    /// A role create/edit assigned a permission-claim key outside the tenant-RBAC catalog (SEC-09)
    /// — the catalog-membership check <c>RoleValidator</c> deliberately leaves to the handler.
    /// </summary>
    public const string UnknownPermissionClaimKey = "ERR-IDENTITY-006";

    /// <summary>
    /// A role edit tried to remove the system-seeded owner-admin role's protected claim, which would
    /// lock the tenant out of staff management (SEC-11).
    /// </summary>
    public const string SystemRoleClaimProtected = "ERR-IDENTITY-007";

    /// <summary>
    /// A staff create/edit for an account other than the tenant's owner-admin submitted a
    /// <c>Username</c> that does not start with the owner-admin's username followed by a hyphen
    /// (SEC-17) — the namespacing check <c>StaffValidator</c> deliberately leaves to the handler,
    /// the same way it leaves <see cref="UsernameAlreadyTaken"/> and <see cref="RoleNotFound"/> to it.
    /// </summary>
    public const string SubordinateUsernamePrefixRequired = "ERR-IDENTITY-008";
}
