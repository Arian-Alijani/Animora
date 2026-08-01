using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The patient form's load-for-edit dispatch target: the single row behind one patient id (DT-02),
/// mirroring <see cref="GetOwnerQuery"/>'s shape.
/// </summary>
public sealed record GetPatientQuery(Guid PatientId) : IQuery<Patient?>;
