using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The owner form's load-for-edit dispatch target — and the owner-scoped patient list's header
/// load (DT-02, this seam's own <c>IOwnerReadStore.GetByIdAsync</c> doc comment) — for the single
/// row behind one owner id.
/// </summary>
public sealed record GetOwnerQuery(Guid OwnerId) : IQuery<Owner?>;
