using Animora.Desktop.Modules.Clients.Data;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="GetPatientListQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetPatientListHandler : IQueryHandler<GetPatientListQuery, PatientPage>
{
    private readonly IPatientReadStore _readStore;

    public GetPatientListHandler(IPatientReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<PatientPage> Handle(GetPatientListQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<PatientPage>(_readStore.GetPageAsync(
            query.OwnerId,
            query.SearchTerm,
            query.AfterId,
            query.Limit,
            cancellationToken));
    }
}
