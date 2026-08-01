using Animora.Desktop.Modules.Clients.Models;

namespace Animora.Desktop.Modules.Clients.Data;

/// <summary>
/// One keyset page of patient rows (CONV-16), shaped by what the patient list screen renders —
/// serving both the global list and any one owner's scoped list — rather than by a table.
/// </summary>
/// <param name="Items">The page's rows, in the store's <c>Id</c> order.</param>
/// <param name="NextCursor">
/// The cursor to pass back as <c>afterId</c> for the following page, or <see langword="null"/> when
/// this page is the last one; "has more" is <c>NextCursor is not null</c> (INV-18), the same
/// convention <c>OwnerPage</c> already uses.
/// </param>
public sealed record PatientPage(IReadOnlyList<Patient> Items, string? NextCursor);

/// <summary>
/// The read seam behind the patient list, patient form and medical-file summary handlers, declared
/// by the module that consumes it and by no one else (DIR-03 applied to the desktop): Stage A
/// composition binds an in-memory fake, Stage C rebinds a Dapper reader (DT-05, INV-20).
/// </summary>
/// <remarks>
/// <para>
/// Carries the medical-file header read (<see cref="GetMedicalFileSummaryAsync"/>) rather than a
/// separate <c>IMedicalFileReadStore</c>: <c>MedicalFile</c> is inside the Patient aggregate
/// (05-domain-model.md), so a second seam would split one aggregate across two interfaces for no
/// gain — the phase 05 TODO header's documented decision (AG-14, INV-18).
/// </para>
/// <para>
/// Split from <see cref="IPatientWriteStore"/> because Stage C binds the two halves to different
/// technologies (Dapper reads, EF Core writes — DT-05), and a merged interface would have to be
/// torn apart then.
/// </para>
/// </remarks>
public interface IPatientReadStore
{
    /// <summary>
    /// Reads one page of patient rows, optionally scoped to one owner and/or narrowed by
    /// <paramref name="searchTerm"/>.
    /// </summary>
    /// <param name="ownerId">
    /// When set, only that owner's patients are returned — the owner-scoped list mode; when
    /// <see langword="null"/>, every patient in the tenant is eligible — the global list mode. One
    /// method serves both modes through this optional filter instead of two near-identical
    /// screens/handlers — the phase 05 TODO's routing decision (AG-14, DESK-ARCH-05, CONV-17).
    /// </param>
    /// <param name="searchTerm">
    /// Matched against name, breed, microchip id and barcode value; <see langword="null"/> or blank
    /// means no filter. An explicit, typed filter rather than a query DSL (CONV-17).
    /// </param>
    /// <param name="afterId">
    /// Exclusive keyset cursor: the previous page's <see cref="PatientPage.NextCursor"/> (a
    /// <see cref="Patient.Id"/> rendered as a string), or <see langword="null"/> for the first page.
    /// The page is ordered by <see cref="Patient.Id"/> for the same reason
    /// <c>IOwnerReadStore.GetPageAsync</c> orders by <c>Owner.Id</c>: no other column on the row is
    /// both unique and collation-stable, and a <c>UUIDv7</c> order is a rough creation-time order at
    /// no extra cost (CONV-01/02).
    /// </param>
    /// <param name="limit">Maximum rows to return, so the virtualized grid never loads all (DT-08).</param>
    /// <param name="cancellationToken">Propagated to the underlying storage call, no different from
    /// every other seam method in this module.</param>
    Task<PatientPage> GetPageAsync(
        Guid? ownerId,
        string? searchTerm,
        string? afterId,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the single row the edit form loads with, or <see langword="null"/> when no patient
    /// carries <paramref name="patientId"/>.
    /// </summary>
    Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the medical-file header the summary screen renders, or <see langword="null"/> when no
    /// patient carries <paramref name="patientId"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="MedicalFileSummary"/> rather than <see cref="Patient"/> itself: the two
    /// read models happen to carry overlapping fields today, but they back distinct screens
    /// (patient list/form vs. medical-file summary) that are free to diverge without disturbing
    /// each other's contract.
    /// </remarks>
    Task<MedicalFileSummary?> GetMedicalFileSummaryAsync(Guid patientId, CancellationToken cancellationToken);
}
