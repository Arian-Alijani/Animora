using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.Models;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="GetOwnerQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetOwnerHandler : IQueryHandler<GetOwnerQuery, Owner?>
{
    private readonly IOwnerReadStore _readStore;

    public GetOwnerHandler(IOwnerReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<Owner?> Handle(GetOwnerQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<Owner?>(_readStore.GetByIdAsync(query.OwnerId, cancellationToken));
    }
}
