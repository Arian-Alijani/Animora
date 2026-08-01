using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Validation.Identity;

namespace Animora.Desktop.Modules.Identity.Data;

// TODO(P1-15): delete this type and rebind IRoleReadStore/IRoleWriteStore to the Dapper-backed
// reader and EF Core-backed writer over the local database (DT-05, INV-20). Nothing but the two
// registration lines in Composition/ServiceCollectionExtensions changes with it (DIR-03).
/// <summary>
/// Satisfies both <see cref="IRoleReadStore"/> and <see cref="IRoleWriteStore"/> over
/// <see cref="IdentitySampleData"/>, mirroring <see cref="InMemoryStaffStore"/>.
/// </summary>
internal sealed class InMemoryRoleStore : IRoleReadStore, IRoleWriteStore
{
    private readonly IdentitySampleData _sampleData;

    public InMemoryRoleStore(IdentitySampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            IReadOnlyList<Role> roles = _sampleData.Roles
                .OrderBy(role => role.DisplayName, StringComparer.Ordinal)
                .Select(ToRole)
                .ToList();

            return Task.FromResult(roles);
        }
    }

    public Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var role = _sampleData.Roles.FirstOrDefault(candidate => candidate.Id == roleId);
            return Task.FromResult(role is null ? null : ToRole(role));
        }
    }

    public Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            return Task.FromResult(_sampleData.Roles.Any(role => role.Id == roleId));
        }
    }

    public Task SaveAsync(Guid roleId, IRoleInput input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            // A create's roleId never matches an existing row, so isSystemRole stays false for it;
            // an edit of the seeded owner-admin role keeps its flag rather than losing it to a
            // freshly-built row that has no way to set it (SEC-11 is the handler's guard, not this
            // seam's, but the flag itself must survive every save regardless).
            var isSystemRole = _sampleData.Roles.FirstOrDefault(role => role.Id == roleId)?.IsSystemRole ?? false;

            _sampleData.Roles.RemoveAll(role => role.Id == roleId);
            _sampleData.Roles.Add(new RoleDefinition(
                roleId,
                input.DisplayName,
                input.PermissionClaimKeys.ToArray(),
                isSystemRole));

            return Task.CompletedTask;
        }
    }

    // Resolves Role.MemberCount at read time (must run inside the caller's Gate lock): the
    // aggregate IRoleReadStore.GetByIdAsync's own doc comment says Stage C computes with a join,
    // never stored on the role row itself.
    private Role ToRole(RoleDefinition role)
    {
        var memberCount = _sampleData.Staff.Count(account => account.RoleId == role.Id);

        return new Role(role.Id, role.DisplayName, role.PermissionClaimKeys, memberCount, role.IsSystemRole);
    }
}
