using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Data;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="SaveStaffMemberCommand"/>: runs <see cref="StaffValidator"/>, then the two
/// lookup-dependent rules the validator deliberately leaves to this handler (SH-05) —
/// <see cref="IdentityErrors.RoleNotFound"/>/<see cref="IdentityErrors.UsernameAlreadyTaken"/> — plus
/// SEC-17's username-namespacing guard, before writing through <see cref="IStaffWriteStore"/>.
/// </summary>
public sealed class SaveStaffMemberHandler : ICommandHandler<SaveStaffMemberCommand, Result<Guid>>
{
    // Stateless and I/O-free (SH-05): one shared instance mirrors SignInHandler's own CredentialValidator.
    private static readonly StaffValidator Validator = new();

    private readonly IStaffReadStore _staffReadStore;
    private readonly IStaffWriteStore _staffWriteStore;
    private readonly IRoleReadStore _roleReadStore;

    public SaveStaffMemberHandler(
        IStaffReadStore staffReadStore,
        IStaffWriteStore staffWriteStore,
        IRoleReadStore roleReadStore)
    {
        _staffReadStore = staffReadStore;
        _staffWriteStore = staffWriteStore;
        _roleReadStore = roleReadStore;
    }

    public async ValueTask<Result<Guid>> Handle(SaveStaffMemberCommand command, CancellationToken cancellationToken)
    {
        var validation = await Validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<Guid>(new Error(IdentityErrors.ValidationFailed, validation.ToString()));
        }

        // One read serves both the RoleNotFound check StaffValidator leaves to this handler and the
        // SEC-17 exemption below, which needs Role.IsSystemRole rather than just existence — the
        // reason IRoleReadStore.ExistsAsync (a cheaper existence-only read) is not used here.
        var role = await _roleReadStore.GetByIdAsync(command.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<Guid>(new Error(IdentityErrors.RoleNotFound));
        }

        var staffId = command.StaffId ?? Guid.CreateVersion7();

        var usernameHolderId = await _staffReadStore.FindIdByUsernameAsync(command.Username, cancellationToken);
        if (usernameHolderId is not null && usernameHolderId != staffId)
        {
            return Result.Failure<Guid>(new Error(IdentityErrors.UsernameAlreadyTaken));
        }

        // SEC-17: an account holding the system role itself is the anchor, exempt from the check on
        // itself. Every other account's username must be namespaced under whoever currently holds it.
        if (!role.IsSystemRole)
        {
            var ownerAdminUsername = await _staffReadStore.FindOwnerAdminUsernameAsync(cancellationToken);

            // No anchor to check against yet (bootstrap edge case: no staff holds the owner-admin
            // role) — nothing to enforce until one exists.
            if (ownerAdminUsername is not null &&
                !command.Username.StartsWith(ownerAdminUsername + "-", StringComparison.Ordinal))
            {
                return Result.Failure<Guid>(new Error(IdentityErrors.SubordinateUsernamePrefixRequired));
            }
        }

        await _staffWriteStore.SaveAsync(staffId, command, command.IsActive, cancellationToken);

        return Result.Success(staffId);
    }
}
