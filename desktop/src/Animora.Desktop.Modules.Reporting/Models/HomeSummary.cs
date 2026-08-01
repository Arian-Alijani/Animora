namespace Animora.Desktop.Modules.Reporting.Models;

/// <summary>
/// The Home screen's read model, carried in storage units: money as <see langword="decimal"/> Rials
/// (INV-05, FIN-03) and instants as UTC (INV-13) — Persian digits, Toman conversion and Jalali dates
/// are applied at the binding edge, never here (DESK-ARCH-14).
/// <para>
/// One shape serves both the module's read seam (<c>IHomeSummaryReadStore</c>) and the response of
/// <c>GetHomeSummaryQuery</c>, instead of a store row plus a near-identical response DTO: the Stage
/// A/C swap changes where these values come from, never what they are, so a second shape would only
/// add a mapping to maintain (AG-14).
/// </para>
/// </summary>
/// <param name="TodayVisitCount">Visits recorded today.</param>
/// <param name="TodayAppointmentCount">Appointments scheduled for today.</param>
/// <param name="OutstandingInvoiceCount">Invoices with an unsettled balance.</param>
/// <param name="TodayRevenueInRials">Today's settled revenue, in Rials.</param>
/// <param name="GeneratedAtUtc">When these values were read, for the screen's freshness line.</param>
public sealed record HomeSummary(
    int TodayVisitCount,
    int TodayAppointmentCount,
    int OutstandingInvoiceCount,
    decimal TodayRevenueInRials,
    DateTime GeneratedAtUtc);
