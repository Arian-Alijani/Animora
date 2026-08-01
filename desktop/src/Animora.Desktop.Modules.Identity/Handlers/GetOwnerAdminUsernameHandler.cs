using Animora.Desktop.Modules.Identity.Data;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="GetOwnerAdminUsernameQuery"/> against the module's own read seam and nothing
/// else (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetOwnerAdminUsernameHandler : IQueryHandler<GetOwnerAdminUsernameQuery, string?>
{
    private readonly IStaffReadStore _readStore;

    public GetOwnerAdminUsernameHandler(IStaffReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<string?> Handle(GetOwnerAdminUsernameQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(_readStore.FindOwnerAdminUsernameAsync(cancellationToken));
    }
}
