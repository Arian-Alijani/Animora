using Animora.Desktop.Modules.Reporting.Models;

namespace Animora.Desktop.Modules.Reporting.Data;

// TODO(P1-21): delete this type and rebind IHomeSummaryReadStore to the Dapper-backed reader over the
// local database (DT-05, INV-20). Nothing but the one registration line in
// Composition/ServiceCollectionExtensions changes with it (DIR-03).
internal sealed class InMemoryHomeSummaryReadStore : IHomeSummaryReadStore
{
    // Deliberately not round numbers: a mid-size clinic's day exercises thousands grouping and an
    // eight-digit money value, so a regression at the Persian-digit/Toman binding edge is visible on
    // the screen itself rather than only in a formatter test.
    private const int TodayVisitCount = 18;
    private const int TodayAppointmentCount = 24;
    private const int OutstandingInvoiceCount = 6;
    private const decimal TodayRevenueInRials = 184_500_000m;

    private readonly TimeProvider _timeProvider;

    public InMemoryHomeSummaryReadStore(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<HomeSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Read through the injected clock rather than DateTime.UtcNow (CONV-06), and hand the value
        // over as UTC (INV-13) — the screen's freshness line goes through JalaliDateFormatter, which
        // rejects any other DateTimeKind.
        return Task.FromResult(new HomeSummary(
            TodayVisitCount,
            TodayAppointmentCount,
            OutstandingInvoiceCount,
            TodayRevenueInRials,
            _timeProvider.GetUtcNow().UtcDateTime));
    }
}
