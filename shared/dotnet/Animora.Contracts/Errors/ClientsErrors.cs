namespace Animora.Contracts.Errors;

/// <summary>
/// Stable <c>ERR-CLIENTS-{NNN}</c> codes the Clients module's handlers return (CONV-13/14/15).
/// </summary>
/// <remarks>
/// Append-only and monotonically numbered (SH-03), mirroring <see cref="IdentityErrors"/>: a code
/// is never renumbered or reused after retirement. Only the two codes this phase's TODO already
/// names are declared now (see
/// <c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> items 18/21) — a later session
/// adds to this list as its own handler needs a new code, the same incremental discipline
/// <see cref="IdentityErrors"/> already follows rather than a codes catalog invented ahead of the
/// handler that returns it.
/// </remarks>
public static class ClientsErrors
{
    /// <summary>
    /// A command's shared <c>FluentValidation</c> validator (CONV-18) — <c>OwnerValidator</c> or
    /// <c>PatientValidator</c> — rejected the submitted input.
    /// </summary>
    /// <remarks>
    /// One shared code for both validators, not one each: this mirrors
    /// <see cref="IdentityErrors.ValidationFailed"/>, which already serves both
    /// <c>StaffValidator</c> and <c>RoleValidator</c> — the failed field names travel in the
    /// result's detail text (CONV-12), so a second code would distinguish nothing a client
    /// actually branches on (INV-07).
    /// </remarks>
    public const string ValidationFailed = "ERR-CLIENTS-001";

    /// <summary>
    /// A patient create/edit referenced an <c>OwnerId</c> that does not exist — the existence
    /// check <c>PatientValidator</c> deliberately leaves to the handler because it needs a
    /// lookup, which an I/O-free shared validator cannot perform (DOM-03, SH-05). Mirrors
    /// <see cref="IdentityErrors.RoleNotFound"/>'s same shape for <c>SaveStaffMemberHandler</c>.
    /// </summary>
    public const string OwnerNotFound = "ERR-CLIENTS-002";
}
