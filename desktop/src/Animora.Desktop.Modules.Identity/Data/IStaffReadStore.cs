using Animora.Desktop.Modules.Identity.Models;

namespace Animora.Desktop.Modules.Identity.Data;

/// <summary>
/// One keyset page of staff rows (CONV-16), shaped by what the staff list screen renders rather
/// than by a table.
/// </summary>
/// <param name="Items">The page's rows, in the store's <c>Username</c> order.</param>
/// <param name="NextCursor">
/// The cursor to pass back as <c>afterUsername</c> for the following page, or <see langword="null"/>
/// when this page is the last one. The wire meta carries a separate <c>hasMore</c> flag (CONV-16);
/// duplicating it here would make two fields answer one question (INV-18), so "has more" is
/// <c>NextCursor is not null</c>.
/// </param>
public sealed record StaffPage(IReadOnlyList<StaffMember> Items, string? NextCursor);

/// <summary>
/// The read seam behind the staff list and staff form handlers, declared by the module that
/// consumes it and by no one else (DIR-03 applied to the desktop): Stage A composition binds an
/// in-memory fake, Stage C rebinds a Dapper reader (DT-05, INV-20), and neither the interface, the
/// handlers, nor the screens change between them.
/// </summary>
/// <remarks>
/// Split from <see cref="IStaffWriteStore"/> because Stage C binds the two halves to different
/// technologies (Dapper reads, EF Core writes — DT-05), and a merged interface would have to be
/// torn apart then.
/// </remarks>
public interface IStaffReadStore
{
    /// <summary>
    /// Reads one page of staff rows, optionally narrowed by <paramref name="searchTerm"/>.
    /// </summary>
    /// <param name="searchTerm">
    /// Matched against full name, username and mobile number; <see langword="null"/> or blank means
    /// no filter. An explicit, typed filter rather than a query DSL (CONV-17).
    /// </param>
    /// <param name="afterUsername">
    /// Exclusive keyset cursor: the previous page's <see cref="StaffPage.NextCursor"/>, or
    /// <see langword="null"/> for the first page. The page is ordered by
    /// <see cref="StaffMember.Username"/> because it is the only unique, collation-stable column on
    /// the row — Persian full names would need an ICU collation SQLite does not carry by default,
    /// and a non-unique sort key cannot be a keyset cursor on its own (CONV-16, DT-08).
    /// </param>
    /// <param name="limit">Maximum rows to return, so the virtualized grid never loads all (DT-08).</param>
    Task<StaffPage> GetPageAsync(
        string? searchTerm,
        string? afterUsername,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the single row the edit form loads with, or <see langword="null"/> when no staff
    /// member carries <paramref name="staffId"/>.
    /// </summary>
    Task<StaffMember?> GetByIdAsync(Guid staffId, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves which account currently holds <paramref name="username"/>, or
    /// <see langword="null"/> when it is free.
    /// </summary>
    /// <remarks>
    /// Returns the holder's id rather than a boolean so the save handler can tell "taken by someone
    /// else" (<c>IdentityErrors.UsernameAlreadyTaken</c>) from "unchanged on the row being edited"
    /// in one round trip — the uniqueness check <c>StaffValidator</c> leaves to the handler (SH-05).
    /// </remarks>
    Task<Guid?> FindIdByUsernameAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the username of whichever staff member currently holds the tenant's system-seeded
    /// owner-admin role (SEC-11), or <see langword="null"/> when no staff member does yet.
    /// </summary>
    /// <remarks>
    /// This is the anchor <c>SaveStaffMemberCommand</c> checks a non-owner-admin username against
    /// (SEC-17): every other account's username must start with this value followed by a hyphen.
    /// Kept on this seam rather than <see cref="IRoleReadStore"/> because resolving it needs a
    /// staff-by-role lookup, which is this interface's concern, not the role catalog's.
    /// </remarks>
    Task<string?> FindOwnerAdminUsernameAsync(CancellationToken cancellationToken);
}
