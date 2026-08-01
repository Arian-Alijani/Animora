using Animora.Desktop.Modules.Clients.Data;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// Handles <see cref="GetOwnerListQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetOwnerListHandler : IQueryHandler<GetOwnerListQuery, OwnerPage>
{
    private readonly IOwnerReadStore _readStore;

    public GetOwnerListHandler(IOwnerReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<OwnerPage> Handle(GetOwnerListQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<OwnerPage>(
            _readStore.GetPageAsync(query.SearchTerm, query.AfterId, query.Limit, cancellationToken));
    }
}
