using Animora.Desktop.Modules.Identity.Data;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// The staff list screen's one dispatch target: one keyset page over <see cref="IStaffReadStore"/>,
/// optionally narrowed by <paramref name="SearchTerm"/> (DT-08, CONV-16).
/// </summary>
/// <param name="SearchTerm">Mirrors <c>IStaffReadStore.GetPageAsync</c>'s <c>searchTerm</c>.</param>
/// <param name="AfterUsername">Mirrors <c>IStaffReadStore.GetPageAsync</c>'s <c>afterUsername</c>.</param>
/// <param name="Limit">Mirrors <c>IStaffReadStore.GetPageAsync</c>'s <c>limit</c>.</param>
public sealed record GetStaffListQuery(string? SearchTerm, string? AfterUsername, int Limit)
    : IQuery<StaffPage>;
