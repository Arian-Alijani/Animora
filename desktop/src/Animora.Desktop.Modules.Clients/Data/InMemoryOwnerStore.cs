using Animora.Desktop.Modules.Clients.Models;
using Animora.SharedKernel.Validation.Clients;

namespace Animora.Desktop.Modules.Clients.Data;

// TODO(P1-16): delete this type and rebind IOwnerReadStore/IOwnerWriteStore to the Dapper-backed
// reader and EF Core-backed writer over the local database (DT-05, INV-20). Nothing but the two
// registration lines in Composition/ServiceCollectionExtensions changes with it (DIR-03).
/// <summary>
/// Satisfies both <see cref="IOwnerReadStore"/> and <see cref="IOwnerWriteStore"/> over
/// <see cref="ClientsSampleData"/>, so a create made through the write half shows up in the read
/// half's next query the way one SQLite table would (DIR-03, DT-12).
/// </summary>
internal sealed class InMemoryOwnerStore : IOwnerReadStore, IOwnerWriteStore
{
    private readonly ClientsSampleData _sampleData;

    public InMemoryOwnerStore(ClientsSampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<OwnerPage> GetPageAsync(
        string? searchTerm,
        string? afterId,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            IEnumerable<Owner> filtered = _sampleData.Owners;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(owner =>
                    owner.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    owner.MobileNumber.Contains(searchTerm, StringComparison.Ordinal) ||
                    (owner.NationalId is not null &&
                        owner.NationalId.Contains(searchTerm, StringComparison.Ordinal)));
            }

            if (!string.IsNullOrEmpty(afterId) && Guid.TryParse(afterId, out var afterOwnerId))
            {
                filtered = filtered.Where(owner => owner.Id.CompareTo(afterOwnerId) > 0);
            }

            // Owner.Id, not FullName/MobileNumber: the only unique, collation-stable column on the
            // row — this seam's own IOwnerReadStore.GetPageAsync doc comment (CONV-16, DT-08).
            var ordered = filtered.OrderBy(owner => owner.Id);

            // One extra row than the page size, so "was there another page" never needs a second
            // count query — exactly what Stage C's Dapper reader will do with LIMIT (limit + 1).
            var window = ordered.Take(limit + 1).ToList();
            var hasMore = window.Count > limit;
            var items = window.Take(limit).ToList();
            var nextCursor = hasMore ? items[^1].Id.ToString() : null;

            return Task.FromResult(new OwnerPage(items, nextCursor));
        }
    }

    public Task<Owner?> GetByIdAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var owner = _sampleData.Owners.FirstOrDefault(candidate => candidate.Id == ownerId);
            return Task.FromResult(owner);
        }
    }

    public Task<bool> ExistsAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            return Task.FromResult(_sampleData.Owners.Any(owner => owner.Id == ownerId));
        }
    }

    public Task SaveAsync(Guid ownerId, IOwnerInput input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            _sampleData.Owners.RemoveAll(owner => owner.Id == ownerId);
            _sampleData.Owners.Add(new Owner(
                ownerId,
                input.FullName,
                input.MobileNumber,
                input.LandlineNumber,
                input.NationalId,
                input.Address,
                input.City,
                input.Notes,
                input.IntakeDateUtc));

            return Task.CompletedTask;
        }
    }
}
