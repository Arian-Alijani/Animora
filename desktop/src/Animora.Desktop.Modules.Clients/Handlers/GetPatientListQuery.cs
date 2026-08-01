using Animora.Desktop.Modules.Clients.Data;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The patient list screen's one dispatch target for both list modes: one keyset page over
/// <see cref="IPatientReadStore"/>, optionally scoped to one owner and/or narrowed by
/// <paramref name="SearchTerm"/> (DT-08, CONV-16/17) — the phase 05 TODO header's "one patient-list
/// route serves both modes" decision (AG-14, DESK-ARCH-05).
/// </summary>
/// <param name="OwnerId">Mirrors <c>IPatientReadStore.GetPageAsync</c>'s <c>ownerId</c>.</param>
/// <param name="SearchTerm">Mirrors <c>IPatientReadStore.GetPageAsync</c>'s <c>searchTerm</c>.</param>
/// <param name="AfterId">Mirrors <c>IPatientReadStore.GetPageAsync</c>'s <c>afterId</c>.</param>
/// <param name="Limit">Mirrors <c>IPatientReadStore.GetPageAsync</c>'s <c>limit</c>.</param>
public sealed record GetPatientListQuery(Guid? OwnerId, string? SearchTerm, string? AfterId, int Limit)
    : IQuery<PatientPage>;
