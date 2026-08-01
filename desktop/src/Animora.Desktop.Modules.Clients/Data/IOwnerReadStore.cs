using Animora.Desktop.Modules.Clients.Models;

namespace Animora.Desktop.Modules.Clients.Data;

/// <summary>
/// One keyset page of owner rows (CONV-16), shaped by what the owner list screen renders rather
/// than by a table.
/// </summary>
/// <param name="Items">The page's rows, in the store's <c>Id</c> order.</param>
/// <param name="NextCursor">
/// The cursor to pass back as <c>afterId</c> for the following page, or <see langword="null"/> when
/// this page is the last one. The wire meta carries a separate <c>hasMore</c> flag (CONV-16);
/// duplicating it here would make two fields answer one question (INV-18), so "has more" is
/// <c>NextCursor is not null</c> — the same convention <c>StaffPage</c> already uses.
/// </param>
public sealed record OwnerPage(IReadOnlyList<Owner> Items, string? NextCursor);

/// <summary>
/// The read seam behind the owner list and owner form handlers, declared by the module that
/// consumes it and by no one else (DIR-03 applied to the desktop): Stage A composition binds an
/// in-memory fake, Stage C rebinds a Dapper reader (DT-05, INV-20), and neither the interface, the
/// handlers, nor the screens change between them.
/// </summary>
/// <remarks>
/// Split from <see cref="IOwnerWriteStore"/> because Stage C binds the two halves to different
/// technologies (Dapper reads, EF Core writes — DT-05), and a merged interface would have to be
/// torn apart then.
/// </remarks>
public interface IOwnerReadStore
{
    /// <summary>
    /// Reads one page of owner rows, optionally narrowed by <paramref name="searchTerm"/>.
    /// </summary>
    /// <param name="searchTerm">
    /// Matched against full name, mobile number and national id; <see langword="null"/> or blank
    /// means no filter. An explicit, typed filter rather than a query DSL (CONV-17).
    /// </param>
    /// <param name="afterId">
    /// Exclusive keyset cursor: the previous page's <see cref="OwnerPage.NextCursor"/> (an
    /// <see cref="Owner.Id"/> rendered as a string), or <see langword="null"/> for the first page.
    /// The page is ordered by <see cref="Owner.Id"/> because it is the only column on the row that
    /// is both unique and collation-stable: <see cref="Owner.MobileNumber"/> is deliberately not
    /// unique (phase 05 TODO item 4's documented answer) and <see cref="Owner.FullName"/> is
    /// Persian free text with no ICU collation available — the same reasoning
    /// <c>IStaffReadStore</c> already applies to ordering by <c>Username</c> instead of full name.
    /// Because <see cref="Owner.Id"/> is a <c>UUIDv7</c>, this order is also a rough creation-time
    /// order, at no extra cost (CONV-01/02).
    /// </param>
    /// <param name="limit">Maximum rows to return, so the virtualized grid never loads all (DT-08).</param>
    /// <param name="cancellationToken">Propagated to the underlying storage call, no different from
    /// every other seam method in this module.</param>
    Task<OwnerPage> GetPageAsync(
        string? searchTerm,
        string? afterId,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the single row the edit form — and the owner-scoped patient list's header — load
    /// with, or <see langword="null"/> when no owner carries <paramref name="ownerId"/>.
    /// </summary>
    Task<Owner?> GetByIdAsync(Guid ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Whether <paramref name="ownerId"/> names an owner — the existence check
    /// <c>PatientValidator</c> deliberately leaves to the patient save handler because it needs a
    /// lookup, which an I/O-free shared validator cannot perform (DOM-03, SH-05); the handler
    /// returns <c>ClientsErrors.OwnerNotFound</c> when this is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Kept separate from <see cref="GetByIdAsync"/> for the same reason
    /// <c>IRoleReadStore.ExistsAsync</c> is: materializing a full <see cref="Owner"/> row costs
    /// fields the caller never reads just to confirm one exists, and every patient save would pay
    /// for it.
    /// </remarks>
    Task<bool> ExistsAsync(Guid ownerId, CancellationToken cancellationToken);
}
