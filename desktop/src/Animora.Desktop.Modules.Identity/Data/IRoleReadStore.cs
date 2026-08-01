using Animora.Desktop.Modules.Identity.Models;

namespace Animora.Desktop.Modules.Identity.Data;

/// <summary>
/// The read seam behind the role-management handlers (DIR-03), split from
/// <see cref="IRoleWriteStore"/> for the reason DT-05 fixes: Dapper reads, EF Core writes.
/// </summary>
public interface IRoleReadStore
{
    /// <summary>
    /// Reads every role in the tenant, ordered by <see cref="Role.DisplayName"/>.
    /// </summary>
    /// <remarks>
    /// Unpaged on purpose: a tenant defines a handful of roles, so this list cannot reach DT-08's
    /// 200-row threshold, and the role screen's claim-assignment panel needs the whole set anyway.
    /// </remarks>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the single role the claim-assignment form loads with, or <see langword="null"/> when
    /// no role carries <paramref name="roleId"/>.
    /// </summary>
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>
    /// Whether <paramref name="roleId"/> names a role in the tenant — the existence check
    /// <c>StaffValidator</c> leaves to the staff save handler (<c>IdentityErrors.RoleNotFound</c>).
    /// </summary>
    /// <remarks>
    /// Kept separate from <see cref="GetByIdAsync"/> because materializing a <see cref="Role"/>
    /// costs the <see cref="Role.MemberCount"/> aggregate the caller does not read, and every staff
    /// save would pay for it.
    /// </remarks>
    Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken);
}
