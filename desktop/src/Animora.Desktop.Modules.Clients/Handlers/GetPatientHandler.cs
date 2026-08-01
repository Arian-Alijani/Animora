using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="GetPatientQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetPatientHandler : IQueryHandler<GetPatientQuery, Patient?>
{
    private readonly IPatientReadStore _readStore;

    public GetPatientHandler(IPatientReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<Patient?> Handle(GetPatientQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<Patient?>(_readStore.GetByIdAsync(query.PatientId, cancellationToken));
    }
}
