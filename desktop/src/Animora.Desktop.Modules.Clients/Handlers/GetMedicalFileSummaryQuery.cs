using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The medical-file summary screen's one dispatch target (playbook step 3): the header row behind
/// one patient id, read through the patient seam rather than a seam of its own — this phase's
/// header decision (AG-14, INV-18) — mirroring <see cref="GetPatientQuery"/>'s shape.
/// </summary>
public sealed record GetMedicalFileSummaryQuery(Guid PatientId) : IQuery<MedicalFileSummary?>;
