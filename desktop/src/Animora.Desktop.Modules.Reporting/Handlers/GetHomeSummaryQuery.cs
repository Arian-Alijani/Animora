using Animora.Desktop.Modules.Reporting.Models;
using Mediator;

namespace Animora.Desktop.Modules.Reporting.Handlers;

/// <summary>
/// This module's request for the Home screen's summary and its public contract surface: another
/// module reaches this data by sending this query, never by referencing the Reporting project
/// (DIR-01/DT-01). A ViewModel sends it instead of reading data itself (DESK-ARCH-02).
/// <para>
/// A query, not a command: it changes nothing, so it carries no idempotency or outbox concern
/// (DESK-ARCH-09 applies to writes only). No parameters — the screen shows the current clinic's
/// today, which the handler's store already scopes.
/// </para>
/// </summary>
public sealed record GetHomeSummaryQuery : IQuery<HomeSummary>;
