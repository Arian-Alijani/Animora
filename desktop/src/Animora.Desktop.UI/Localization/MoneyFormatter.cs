namespace Animora.Desktop.UI.Localization;

/// <summary>
/// IRR/Toman money formatting for the UI binding edge (CONV-07, INV-05, FIN-19). The ledger and
/// every handler/domain value are persisted in Rials as <see langword="decimal"/> (FIN-03: `amount
/// decimal(18,2)`, `currency` fixed `IRR`); this type is the only place that value becomes the
/// Persian-digit string a view renders, in either of the two units the product owner's reference
/// screens use side by side (TECH_STACK §1: "Currency: IRR / Toman").
/// </summary>
/// <remarks>
/// 1 Toman = 10 Rials is Iran's standard, unambiguous unit relationship — not a product decision
/// this type makes. Rounding to the nearest whole unit uses banker's rounding (FIN-20); IRR/Toman
/// has no fractional sub-unit in practice (FIN-19), so both outputs are integers.
/// </remarks>
public sealed class MoneyFormatter
{
    private const decimal RialsPerToman = 10m;

    private readonly PersianNumberFormatter _numberFormatter;

    public MoneyFormatter(PersianNumberFormatter numberFormatter)
    {
        _numberFormatter = numberFormatter;
    }

    /// <summary>Formats an amount already in Rials (the ledger's persisted unit) with the "ریال"
    /// suffix, e.g. <c>864500000</c> → "۸۶۴,۵۰۰,۰۰۰ ریال".</summary>
    public string FormatRials(decimal amountInRials) => $"{FormatWholeUnits(amountInRials)} ریال";

    /// <summary>Converts an amount in Rials to Tomans (÷10) and formats it with the "تومان" suffix,
    /// e.g. <c>864500000</c> Rials → "۸۶,۴۵۰,۰۰۰ تومان" (design-reference.md §7's example figure).</summary>
    public string FormatTomans(decimal amountInRials) => $"{FormatWholeUnits(amountInRials / RialsPerToman)} تومان";

    private string FormatWholeUnits(decimal amount)
    {
        decimal rounded = Math.Round(amount, 0, MidpointRounding.ToEven);
        return _numberFormatter.FormatNumber((long)rounded);
    }
}
