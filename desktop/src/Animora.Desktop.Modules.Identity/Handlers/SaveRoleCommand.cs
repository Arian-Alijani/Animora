using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Identity;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// The role-management screen's one dispatch target for both create and edit (playbook step 3).
/// Implements <see cref="IRoleInput"/> directly (CONV-18, INV-02) so <see cref="RoleValidator"/> runs
/// against the command itself.
/// </summary>
/// <param name="RoleId">
/// <see langword="null"/> for a create, in which case the handler assigns a fresh <c>UUIDv7</c>
/// (INV-03); the row being edited otherwise.
/// </param>
public sealed record SaveRoleCommand(
    Guid? RoleId,
    string DisplayName,
    IReadOnlyCollection<string> PermissionClaimKeys) : IRoleInput, ICommand<Result<Guid>>;
