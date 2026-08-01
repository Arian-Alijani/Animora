using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Clients.Data;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Clients;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="SaveOwnerCommand"/>: runs <see cref="OwnerValidator"/>, then writes through
/// <see cref="IOwnerWriteStore"/>. No lookup-dependent rule runs between the two — unlike
/// <c>SaveStaffMemberHandler</c>'s role/username checks, phase 05 TODO item 4's documented answer
/// means a duplicate <see cref="IOwnerInput.MobileNumber"/> is not one of this handler's concerns.
/// </summary>
public sealed class SaveOwnerHandler : ICommandHandler<SaveOwnerCommand, Result<Guid>>
{
    // Stateless and I/O-free (SH-05): one shared instance mirrors SaveStaffMemberHandler's own
    // static StaffValidator.
    private static readonly OwnerValidator Validator = new();

    private readonly IOwnerWriteStore _writeStore;

    public SaveOwnerHandler(IOwnerWriteStore writeStore)
    {
        _writeStore = writeStore;
    }

    public async ValueTask<Result<Guid>> Handle(SaveOwnerCommand command, CancellationToken cancellationToken)
    {
        var validation = await Validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<Guid>(new Error(ClientsErrors.ValidationFailed, validation.ToString()));
        }

        var ownerId = command.OwnerId ?? Guid.CreateVersion7();

        await _writeStore.SaveAsync(ownerId, command, cancellationToken);

        return Result.Success(ownerId);
    }
}
