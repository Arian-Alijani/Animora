using Animora.Desktop.Modules.Identity.Models;
using Animora.SharedKernel.Validation.Identity;

namespace Animora.Desktop.Modules.Identity.Data;

// TODO(P1-15): delete this type and rebind IStaffReadStore/IStaffWriteStore to the Dapper-backed
// reader and EF Core-backed writer over the local database (DT-05, INV-20). Nothing but the two
// registration lines in Composition/ServiceCollectionExtensions changes with it (DIR-03).
/// <summary>
/// Satisfies both <see cref="IStaffReadStore"/> and <see cref="IStaffWriteStore"/> over
/// <see cref="IdentitySampleData"/>, so a create made through the write half shows up in the read
/// half's next query the way one SQLite table would (DIR-03, DT-12).
/// </summary>
internal sealed class InMemoryStaffStore : IStaffReadStore, IStaffWriteStore
{
    private readonly IdentitySampleData _sampleData;

    public InMemoryStaffStore(IdentitySampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<StaffPage> GetPageAsync(
        string? searchTerm,
        string? afterUsername,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            IEnumerable<StaffAccount> filtered = _sampleData.Staff;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(account =>
                    account.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    account.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    account.MobileNumber.Contains(searchTerm, StringComparison.Ordinal));
            }

            if (!string.IsNullOrEmpty(afterUsername))
            {
                filtered = filtered.Where(account => string.CompareOrdinal(account.Username, afterUsername) > 0);
            }

            // Username, not full name: the only unique, ordinal-stable column on the row, and the
            // one FindIdByUsernameAsync and FindOwnerAdminUsernameAsync already key on (CONV-16, DT-08).
            var ordered = filtered.OrderBy(account => account.Username, StringComparer.Ordinal);

            // One extra row than the page size, so "was there another page" never needs a second
            // count query — exactly what Stage C's Dapper reader will do with LIMIT (limit + 1).
            var window = ordered.Take(limit + 1).ToList();
            var hasMore = window.Count > limit;
            var items = window.Take(limit).Select(ToStaffMember).ToList();
            var nextCursor = hasMore ? items[^1].Username : null;

            return Task.FromResult(new StaffPage(items, nextCursor));
        }
    }

    public Task<StaffMember?> GetByIdAsync(Guid staffId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var account = _sampleData.Staff.FirstOrDefault(candidate => candidate.Id == staffId);
            return Task.FromResult(account is null ? null : ToStaffMember(account));
        }
    }

    public Task<Guid?> FindIdByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var account = _sampleData.Staff.FirstOrDefault(
                candidate => string.Equals(candidate.Username, username, StringComparison.Ordinal));
            return Task.FromResult<Guid?>(account?.Id);
        }
    }

    public Task<string?> FindOwnerAdminUsernameAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var ownerAdminRole = _sampleData.Roles.FirstOrDefault(role => role.IsSystemRole);
            if (ownerAdminRole is null)
            {
                return Task.FromResult<string?>(null);
            }

            var ownerAdmin = _sampleData.Staff.FirstOrDefault(account => account.RoleId == ownerAdminRole.Id);
            return Task.FromResult(ownerAdmin?.Username);
        }
    }

    public Task SaveAsync(Guid staffId, IStaffInput input, bool isActive, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            _sampleData.Staff.RemoveAll(account => account.Id == staffId);
            _sampleData.Staff.Add(new StaffAccount(
                staffId,
                input.FullName,
                input.Username,
                input.MobileNumber,
                input.Email,
                input.RoleId,
                isActive));

            return Task.CompletedTask;
        }
    }

    // Resolves Role.DisplayName at read time (must run inside the caller's Gate lock): the join
    // Stage C's Dapper reader performs with one query (DT-05), StaffMember's own doc comment on
    // RoleDisplayName.
    private StaffMember ToStaffMember(StaffAccount account)
    {
        var roleDisplayName = _sampleData.Roles
            .FirstOrDefault(role => role.Id == account.RoleId)?
            .DisplayName ?? string.Empty;

        return new StaffMember(
            account.Id,
            account.FullName,
            account.Username,
            account.MobileNumber,
            account.Email,
            account.RoleId,
            roleDisplayName,
            account.IsActive);
    }
}
