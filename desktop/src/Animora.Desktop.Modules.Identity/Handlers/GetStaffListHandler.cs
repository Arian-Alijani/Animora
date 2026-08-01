using Animora.Desktop.Modules.Identity.Data;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="GetStaffListQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetStaffListHandler : IQueryHandler<GetStaffListQuery, StaffPage>
{
    private readonly IStaffReadStore _readStore;

    public GetStaffListHandler(IStaffReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<StaffPage> Handle(GetStaffListQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<StaffPage>(
            _readStore.GetPageAsync(query.SearchTerm, query.AfterUsername, query.Limit, cancellationToken));
    }
}
