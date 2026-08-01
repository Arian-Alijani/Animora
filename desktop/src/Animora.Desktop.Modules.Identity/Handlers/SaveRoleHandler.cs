using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="SaveRoleCommand"/>: runs <see cref="RoleValidator"/>, then the two
/// lookup-dependent rules the validator deliberately leaves to this handler (SH-05) — catalog
/// membership (SEC-09) and the system role's protected claim (SEC-11) — before writing through
/// <see cref="IRoleWriteStore"/>.
/// </summary>
public sealed class SaveRoleHandler : ICommandHandler<SaveRoleCommand, Result<Guid>>
{
    // Stateless and I/O-free (SH-05): one shared instance mirrors SaveStaffMemberHandler's own StaffValidator.
    private static readonly RoleValidator Validator = new();

    private readonly IRoleReadStore _readStore;
    private readonly IRoleWriteStore _writeStore;

    public SaveRoleHandler(IRoleReadStore readStore, IRoleWriteStore writeStore)
    {
        _readStore = readStore;
        _writeStore = writeStore;
    }

    public async ValueTask<Result<Guid>> Handle(SaveRoleCommand command, CancellationToken cancellationToken)
    {
        var validation = Validator.Validate(command);
        if (!validation.IsValid)
        {
            return Result.Failure<Guid>(new Error(IdentityErrors.ValidationFailed, validation.ToString()));
        }

        var unknownKey = command.PermissionClaimKeys.FirstOrDefault(key => !PermissionCatalog.IsKnownKey(key));
        if (unknownKey is not null)
        {
            return Result.Failure<Guid>(new Error(IdentityErrors.UnknownPermissionClaimKey, unknownKey));
        }

        // A create can never target the system-seeded owner-admin role (InMemoryRoleStore.SaveAsync
        // never assigns the flag to a new row), so the SEC-11 guard only has to look at an edit.
        if (command.RoleId is { } existingRoleId)
        {
            var existingRole = await _readStore.GetByIdAsync(existingRoleId, cancellationToken);
            if (existingRole is { IsSystemRole: true } &&
                !command.PermissionClaimKeys.Contains(PermissionCatalog.OwnerAdminProtectedClaimKey, StringComparer.Ordinal))
            {
                return Result.Failure<Guid>(new Error(IdentityErrors.SystemRoleClaimProtected));
            }
        }

        var roleId = command.RoleId ?? Guid.CreateVersion7();

        await _writeStore.SaveAsync(roleId, command, cancellationToken);

        return Result.Success(roleId);
    }
}
