using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// The staff form's one dispatch target for both create and edit (playbook step 3). Implements
/// <see cref="IStaffInput"/> directly (CONV-18, INV-02) so <see cref="StaffValidator"/> runs against
/// the command itself.
/// </summary>
/// <param name="StaffId">
/// <see langword="null"/> for a create, in which case the handler assigns a fresh <c>UUIDv7</c>
/// (INV-03); the row being edited otherwise.
/// </param>
/// <param name="IsActive">
/// Whether the account can sign in (see <see cref="Data.IStaffWriteStore.SaveAsync"/>) — a status
/// flag, not part of <see cref="IStaffInput"/>'s shape.
/// </param>
public sealed record SaveStaffMemberCommand(
    Guid? StaffId,
    string FullName,
    string Username,
    string MobileNumber,
    string? Email,
    Guid RoleId,
    bool IsActive) : IStaffInput, ICommand<Result<Guid>>;
