using System.Globalization;
using System.Text;

namespace Animora.Desktop.UI.Localization;

/// <summary>
/// Persian-Indic digit and number formatting for the UI binding edge (design-reference.md §2:
/// "Digits are Persian-Indic in every position — counts, money, times, dates, chart axis labels,
/// percentages"). Domain/handler code keeps plain <see langword="long"/>/<see langword="decimal"/>
/// values; this is the only place ASCII digits become Persian-Indic ones (DESK-ARCH-14). Reused by
/// <see cref="JalaliDateFormatter"/> and <c>MoneyFormatter</c> (item 27) for the same reason.
/// </summary>
public sealed class PersianNumberFormatter
{
    private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";

    /// <summary>Replaces every ASCII digit 0-9 with its Persian-Indic equivalent; every other
    /// character (separators, currency words, RTL marks) passes through untouched.</summary>
    public static string ToPersianDigits(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            builder.Append(character is >= '0' and <= '9' ? PersianDigits[character - '0'] : character);
        }

        return builder.ToString();
    }

    /// <summary>Thousands-grouped integer with Persian-Indic digits, e.g. <c>86450000</c> →
    /// "۸۶,۴۵۰,۰۰۰". The grouping separator is a plain comma, matching the values observed in
    /// design-reference.md §2 ("۱,۲۸۴", "۸۶,۴۵۰,۰۰۰") rather than fa-IR's default "٬" (U+066C).</summary>
    public static string FormatNumber(long value) => ToPersianDigits(value.ToString("#,##0", CultureInfo.InvariantCulture));

    /// <summary>Percentage with Persian-Indic digits and an ASCII "%", e.g.
    /// <c>FormatPercent(12)</c> → "۱۲%". <paramref name="value"/> is the percentage itself
    /// (<c>12</c>, not <c>0.12</c>).</summary>
    public static string FormatPercent(decimal value, int decimalDigits = 0)
    {
        string formatted = value.ToString(decimalDigits > 0 ? $"F{decimalDigits}" : "F0", CultureInfo.InvariantCulture);
        return ToPersianDigits(formatted) + "%";
    }
}
