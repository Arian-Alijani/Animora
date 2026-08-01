using Animora.Desktop.Modules.Clients.Data;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The owner list screen's one dispatch target: one keyset page over <see cref="IOwnerReadStore"/>,
/// optionally narrowed by <paramref name="SearchTerm"/> (DT-08, CONV-16), with FTS5 left to phase 16
/// (the phase 05 TODO's own scope note on this item).
/// </summary>
/// <param name="SearchTerm">Mirrors <c>IOwnerReadStore.GetPageAsync</c>'s <c>searchTerm</c>.</param>
/// <param name="AfterId">Mirrors <c>IOwnerReadStore.GetPageAsync</c>'s <c>afterId</c>.</param>
/// <param name="Limit">Mirrors <c>IOwnerReadStore.GetPageAsync</c>'s <c>limit</c>.</param>
public sealed record GetOwnerListQuery(string? SearchTerm, string? AfterId, int Limit)
    : IQuery<OwnerPage>;
