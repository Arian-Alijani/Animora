using Animora.Desktop.Modules.Reporting.Data;
using Animora.Desktop.Modules.Reporting.Models;
using Mediator;

namespace Animora.Desktop.Modules.Reporting.Handlers;

/// <summary>
/// Handles <see cref="GetHomeSummaryQuery"/> against the module's own read seam and nothing else: no
/// <c>DbContext</c>, no <c>IDbConnection</c>, no <c>HttpClient</c> (DT-02/DT-05). Because
/// <see cref="IHomeSummaryReadStore"/> is the only dependency, the Stage C swap to real local
/// persistence is a composition change and this file stays untouched (DIR-03, DT-03).
/// </summary>
public sealed class GetHomeSummaryHandler : IQueryHandler<GetHomeSummaryQuery, HomeSummary>
{
    private readonly IHomeSummaryReadStore _readStore;

    public GetHomeSummaryHandler(IHomeSummaryReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<HomeSummary> Handle(GetHomeSummaryQuery query, CancellationToken cancellationToken)
    {
        // No projection or business rule to add: a read this thin stays a pass-through so the seam,
        // not the handler, is what later phases replace. Wrapping the Task keeps the caller on
        // Mediator's ValueTask surface without an async state machine for a synchronous fake.
        return new ValueTask<HomeSummary>(_readStore.GetSummaryAsync(cancellationToken));
    }
}
