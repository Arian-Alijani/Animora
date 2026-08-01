using Animora.Desktop.Modules.Identity.Models;
using Mediator;

namespace Animora.Desktop.Modules.Identity.Handlers;

/// <summary>The staff form's load-for-edit dispatch target: the single row behind one staff id (DT-02).</summary>
public sealed record GetStaffMemberQuery(Guid StaffId) : IQuery<StaffMember?>;
