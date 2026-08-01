using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="GetRolesQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetRolesHandler : IQueryHandler<GetRolesQuery, IReadOnlyList<Role>>
{
    private readonly IRoleReadStore _readStore;

    public GetRolesHandler(IRoleReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<IReadOnlyList<Role>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<IReadOnlyList<Role>>(_readStore.GetAllAsync(cancellationToken));
    }
}
