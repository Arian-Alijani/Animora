using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="SignInQuery"/>: runs <see cref="CredentialValidator"/>, resolves the account
/// through the local credential seam, and projects the role's claims for the session (DT-03, DT-12).
/// </summary>
public sealed class SignInHandler : IQueryHandler<SignInQuery, Result<SignedInStaff>>
{
    // Stateless and I/O-free (SH-05): one shared instance is enough, matching how the module's
    // other handlers (item 22) reuse StaffValidator/RoleValidator.
    private static readonly CredentialValidator Validator = new();

    private readonly IStaffCredentialReadStore _credentialReadStore;
    private readonly IStaffReadStore _staffReadStore;
    private readonly IRoleReadStore _roleReadStore;

    public SignInHandler(
        IStaffCredentialReadStore credentialReadStore,
        IStaffReadStore staffReadStore,
        IRoleReadStore roleReadStore)
    {
        _credentialReadStore = credentialReadStore;
        _staffReadStore = staffReadStore;
        _roleReadStore = roleReadStore;
    }

    public async ValueTask<Result<SignedInStaff>> Handle(SignInQuery query, CancellationToken cancellationToken)
    {
        var validation = await Validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<SignedInStaff>(new Error(IdentityErrors.ValidationFailed, validation.ToString()));
        }

        // TODO(P2): replace this local lookup with a call to the server's sign-in endpoint (DT-12,
        // SEC-01, SEC-03). One generic code covers an unknown username and a wrong password alike,
        // so the form can never be used to enumerate valid accounts.
        var credential = await _credentialReadStore.FindByUsernameAsync(query.Username, cancellationToken);
        if (credential is null || !string.Equals(credential.Password, query.Password, StringComparison.Ordinal))
        {
            return Result.Failure<SignedInStaff>(new Error(IdentityErrors.InvalidCredentials));
        }

        // The credential seam and the staff seam agree on IStaffCredentialReadStore.FindByUsernameAsync's
        // contract, so a missing row here would be a seeding bug, not a real user-facing case; it is
        // still mapped to the same generic code rather than left to throw.
        var staff = await _staffReadStore.GetByIdAsync(credential.StaffId, cancellationToken);
        if (staff is null)
        {
            return Result.Failure<SignedInStaff>(new Error(IdentityErrors.InvalidCredentials));
        }

        if (!staff.IsActive)
        {
            return Result.Failure<SignedInStaff>(new Error(IdentityErrors.AccountInactive));
        }

        var role = await _roleReadStore.GetByIdAsync(staff.RoleId, cancellationToken);

        return Result.Success(new SignedInStaff(
            staff.Id,
            staff.FullName,
            staff.RoleId,
            staff.RoleDisplayName,
            role?.PermissionClaimKeys ?? []));
    }
}
