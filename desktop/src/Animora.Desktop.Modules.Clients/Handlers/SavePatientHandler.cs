using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Clients.Data;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Clients;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="SavePatientCommand"/>: runs <see cref="PatientValidator"/>, then the one
/// lookup-dependent rule the validator deliberately leaves to this handler (SH-05, DOM-03) —
/// <see cref="ClientsErrors.OwnerNotFound"/> when <see cref="SavePatientCommand.OwnerId"/> names no
/// owner — before writing through <see cref="IPatientWriteStore"/>. Mirrors
/// <c>SaveStaffMemberHandler</c>'s validate-then-lookup-then-write shape.
/// </summary>
public sealed class SavePatientHandler : ICommandHandler<SavePatientCommand, Result<Guid>>
{
    // Stateless and I/O-free (SH-05): one shared instance mirrors SaveOwnerHandler's own static
    // OwnerValidator.
    private static readonly PatientValidator Validator = new();

    private readonly IPatientWriteStore _patientWriteStore;
    private readonly IOwnerReadStore _ownerReadStore;

    public SavePatientHandler(IPatientWriteStore patientWriteStore, IOwnerReadStore ownerReadStore)
    {
        _patientWriteStore = patientWriteStore;
        _ownerReadStore = ownerReadStore;
    }

    public async ValueTask<Result<Guid>> Handle(SavePatientCommand command, CancellationToken cancellationToken)
    {
        var validation = await Validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<Guid>(new Error(ClientsErrors.ValidationFailed, validation.ToString()));
        }

        // IOwnerReadStore.ExistsAsync rather than GetByIdAsync: this check never reads a field off
        // the owner row, only confirms one exists (DOM-03), the same reasoning IRoleReadStore's own
        // ExistsAsync already documents.
        var ownerExists = await _ownerReadStore.ExistsAsync(command.OwnerId, cancellationToken);
        if (!ownerExists)
        {
            return Result.Failure<Guid>(new Error(ClientsErrors.OwnerNotFound));
        }

        var patientId = command.PatientId ?? Guid.CreateVersion7();

        await _patientWriteStore.SaveAsync(
            patientId,
            command,
            command.IsBirthDateEstimated,
            command.IsSterilized,
            cancellationToken);

        return Result.Success(patientId);
    }
}
