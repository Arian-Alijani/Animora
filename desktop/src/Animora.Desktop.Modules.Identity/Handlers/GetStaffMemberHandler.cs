using Animora.Desktop.Modules.Identity.Data;
using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>
/// Handles <see cref="GetStaffMemberQuery"/> against the module's own read seam and nothing else
/// (DT-02/DT-05); the Stage C swap to the Dapper-backed reader is a composition change only.
/// </summary>
public sealed class GetStaffMemberHandler : IQueryHandler<GetStaffMemberQuery, StaffMember?>
{
    private readonly IStaffReadStore _readStore;

    public GetStaffMemberHandler(IStaffReadStore readStore)
    {
        _readStore = readStore;
    }

    public ValueTask<StaffMember?> Handle(GetStaffMemberQuery query, CancellationToken cancellationToken)
    {
        return new ValueTask<StaffMember?>(_readStore.GetByIdAsync(query.StaffId, cancellationToken));
    }
}
