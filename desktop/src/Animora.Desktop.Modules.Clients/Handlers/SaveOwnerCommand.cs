using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Clients;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The owner form's one dispatch target for both create and edit (playbook step 3). Implements
/// <see cref="IOwnerInput"/> directly (CONV-18, INV-02) so <see cref="OwnerValidator"/> runs against
/// the command itself.
/// </summary>
/// <param name="OwnerId">
/// <see langword="null"/> for a create, in which case the handler assigns a fresh <c>UUIDv7</c>
/// (INV-03); the row being edited otherwise.
/// </param>
/// <param name="FullName">Mirrors <see cref="IOwnerInput.FullName"/>.</param>
/// <param name="MobileNumber">
/// Mirrors <see cref="IOwnerInput.MobileNumber"/>. No duplicate-mobile rejection runs here — phase
/// 05 TODO item 4's documented answer, also recorded on <see cref="IOwnerInput.MobileNumber"/>'s own
/// doc comment.
/// </param>
/// <param name="LandlineNumber">Mirrors <see cref="IOwnerInput.LandlineNumber"/>.</param>
/// <param name="NationalId">Mirrors <see cref="IOwnerInput.NationalId"/>.</param>
/// <param name="Address">Mirrors <see cref="IOwnerInput.Address"/>.</param>
/// <param name="City">Mirrors <see cref="IOwnerInput.City"/>.</param>
/// <param name="Notes">Mirrors <see cref="IOwnerInput.Notes"/>.</param>
/// <param name="IntakeDateUtc">Mirrors <see cref="IOwnerInput.IntakeDateUtc"/>.</param>
public sealed record SaveOwnerCommand(
    Guid? OwnerId,
    string FullName,
    string MobileNumber,
    string? LandlineNumber,
    string? NationalId,
    string? Address,
    string? City,
    string? Notes,
    DateTime IntakeDateUtc) : IOwnerInput, ICommand<Result<Guid>>;
