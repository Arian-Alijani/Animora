using Animora.Desktop.Modules.Reporting.Models;

namespace Animora.Desktop.Modules.Reporting.Data;

/// <summary>
/// The read seam behind <c>GetHomeSummaryHandler</c>, declared by the module that consumes it and by
/// no one else (DIR-03 applied to the desktop): Stage A composition binds an in-memory fake, Stage C
/// rebinds a Dapper reader (INV-20), and neither the interface, the handler, nor the screen changes
/// between them. Every module UI phase copies this shape rather than re-deriving it.
/// <para>
/// One method, shaped by what the screen asks for rather than by a table, and asynchronous with a
/// cancellation token because the Stage C implementation is a SQLite round trip.
/// </para>
/// </summary>
public interface IHomeSummaryReadStore
{
    Task<HomeSummary> GetSummaryAsync(CancellationToken cancellationToken);
}
