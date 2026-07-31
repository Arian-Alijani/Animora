using System.Globalization;
using Animora.Desktop.UI.Localization;
using Avalonia.Data.Converters;

namespace Animora.Desktop.UI.Converters;

/// <summary>
/// One-way <see cref="IValueConverter"/> binding wrapper over <see cref="JalaliDateFormatter"/>
/// (CONV-05, DESK-ARCH-14). Holds no formatting logic of its own — every mode below delegates
/// straight to the formatter method it names. Bind a UTC <see cref="DateTime"/> and select the
/// mode with <c>ConverterParameter</c> (case-insensitive): <c>Date</c> (default), <c>LongDate</c>,
/// <c>Time</c>, <c>DateTime</c>, <c>RelativeDay</c>.
/// </summary>
public sealed class JalaliDateConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML, backed by the wall-clock
    /// <see cref="TimeProvider.System"/>. Module composition can instead register the underlying
    /// <see cref="JalaliDateFormatter"/> in DI and construct a converter over it where a
    /// deterministic clock matters.</summary>
    public static readonly JalaliDateConverter Instance =
        new(new JalaliDateFormatter(TimeProvider.System));

    private readonly JalaliDateFormatter _formatter;

    public JalaliDateConverter(JalaliDateFormatter formatter)
    {
        _formatter = formatter;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime utcValue)
        {
            return null;
        }

        string mode = parameter as string ?? string.Empty;
        return mode switch
        {
            _ when mode.Equals("longdate", StringComparison.OrdinalIgnoreCase) => _formatter.FormatLongDate(utcValue),
            _ when mode.Equals("time", StringComparison.OrdinalIgnoreCase) => _formatter.FormatTime(utcValue),
            _ when mode.Equals("datetime", StringComparison.OrdinalIgnoreCase) => _formatter.FormatDateTime(utcValue),
            _ when mode.Equals("relativeday", StringComparison.OrdinalIgnoreCase) => _formatter.FormatRelativeDay(utcValue),
            _ => _formatter.FormatDate(utcValue),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(JalaliDateConverter)} is one-way only (CONV-05: Jalali conversion happens at the binding edge, never back into a UTC domain value).");
}
