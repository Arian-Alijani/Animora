using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="GetMedicalFileSummaryQuery"/> against <see cref="IPatientReadStore"/>'s own
/// header read and nothing else (DT-02/DT-05) — no separate medical-file seam, per this phase's
/// header decision (AG-14, INV-18); the Stage C swap to the Dapper-backed reader is a composition
/// change only.
/// </summary>
public sealed class GetMedicalFileSummaryHandler : IQueryHandler<GetMedicalFileSummaryQuery, MedicalFileSummary?>
{
    private readonly IPatientReadStore _readStore;

    public GetMedicalFileSummaryHandler(IPatientReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<MedicalFileSummary?> Handle(GetMedicalFileSummaryQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<MedicalFileSummary?>(
            _readStore.GetMedicalFileSummaryAsync(query.PatientId, cancellationToken));
    }
}
