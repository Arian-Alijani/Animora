using System.Globalization;

namespace Animora.Desktop.UI.Localization;

/// <summary>
/// Jalali date/time formatting over <see cref="PersianCalendar"/> at the UI binding edge
/// (CONV-05, DESK-ARCH-14). Domain and handler code always works in UTC (CONV-04, INV-13); this
/// type is where a UTC instant becomes the Jalali string a view actually renders. Takes a
/// <see cref="TimeProvider"/> instead of reading <see cref="DateTime.Now"/>/<see cref="DateTime.UtcNow"/>
/// directly (CONV-06), so <see cref="FormatRelativeDay"/> is deterministic under test.
/// </summary>
public sealed class JalaliDateFormatter
{
    private static readonly PersianCalendar Calendar = new();

    private static readonly string[] MonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند",
    };

    private readonly TimeProvider _timeProvider;
    private readonly PersianNumberFormatter _numberFormatter;

    public JalaliDateFormatter(TimeProvider timeProvider, PersianNumberFormatter numberFormatter)
    {
        _timeProvider = timeProvider;
        _numberFormatter = numberFormatter;
    }

    /// <summary>Numeric Jalali date, e.g. "۱۴۰۳/۰۴/۱۰".</summary>
    public string FormatDate(DateTime utcValue)
    {
        var local = ToLocal(utcValue);
        var year = Calendar.GetYear(local);
        var month = Calendar.GetMonth(local);
        var day = Calendar.GetDayOfMonth(local);
        return _numberFormatter.ToPersianDigits($"{year:0000}/{month:00}/{day:00}");
    }

    /// <summary>Long-form Jalali date with the month name, e.g. "۱۰ تیر ۱۴۰۳".</summary>
    public string FormatLongDate(DateTime utcValue)
    {
        var local = ToLocal(utcValue);
        var year = Calendar.GetYear(local);
        var month = Calendar.GetMonth(local);
        var day = Calendar.GetDayOfMonth(local);
        return
            $"{_numberFormatter.ToPersianDigits(day.ToString(CultureInfo.InvariantCulture))} " +
            $"{MonthNames[month - 1]} " +
            $"{_numberFormatter.ToPersianDigits(year.ToString(CultureInfo.InvariantCulture))}";
    }

    /// <summary>24-hour local clock time, e.g. "۱۴:۳۰".</summary>
    public string FormatTime(DateTime utcValue) =>
        _numberFormatter.ToPersianDigits(ToLocal(utcValue).ToString("HH:mm", CultureInfo.InvariantCulture));

    /// <summary>"<see cref="FormatDate"/> <see cref="FormatTime"/>" combined.</summary>
    public string FormatDateTime(DateTime utcValue) => $"{FormatDate(utcValue)} {FormatTime(utcValue)}";

    /// <summary>"امروز" / "دیروز" / "فردا" for the three days around <see cref="TimeProvider"/>'s
    /// current instant, else falls back to <see cref="FormatDate"/>.</summary>
    public string FormatRelativeDay(DateTime utcValue)
    {
        var day = ToLocal(utcValue).Date;
        var today = ToLocal(_timeProvider.GetUtcNow().UtcDateTime).Date;
        return (day - today).Days switch
        {
            0 => "امروز",
            -1 => "دیروز",
            1 => "فردا",
            _ => FormatDate(utcValue),
        };
    }

    private DateTime ToLocal(DateTime utcValue)
    {
        // Enforced here rather than left implicit: every caller sits at a ViewModel-to-View binding
        // edge where the value must already be UTC (CONV-04/DESK-ARCH-14) — a Local/Unspecified
        // Kind means an upstream bug, not a value this formatter should silently reinterpret.
        if (utcValue.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Value must be UTC (CONV-04/DESK-ARCH-14).", nameof(utcValue));
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, _timeProvider.LocalTimeZone);
    }
}
