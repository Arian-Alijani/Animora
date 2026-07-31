using System.Globalization;
using Animora.Desktop.UI.Localization;
using Avalonia.Data.Converters;

namespace Animora.Desktop.UI.Converters;

/// <summary>
/// One-way <see cref="IValueConverter"/> binding wrapper over <see cref="PersianNumberFormatter"/>
/// (design-reference.md §2). Holds no formatting logic of its own. Select the mode with
/// <c>ConverterParameter</c> (case-insensitive): <c>Number</c> (default) groups a numeric value
/// with thousands separators via <see cref="PersianNumberFormatter.FormatNumber"/>; <c>Percent</c>
/// formats a numeric value via <see cref="PersianNumberFormatter.FormatPercent"/> (append
/// <c>:{decimalDigits}</c>, e.g. <c>Percent:1</c>, for a non-zero decimal count); <c>Digits</c>
/// Persian-digit-izes the value's own string form via
/// <see cref="PersianNumberFormatter.ToPersianDigits"/> without any grouping.
/// </summary>
public sealed class PersianNumberConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML.</summary>
    public static readonly PersianNumberConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        string[] modeAndArgument = (parameter as string ?? string.Empty).Split(':', 2);
        string mode = modeAndArgument[0];

        if (mode.Equals("digits", StringComparison.OrdinalIgnoreCase))
        {
            return PersianNumberFormatter.ToPersianDigits(System.Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
        }

        if (mode.Equals("percent", StringComparison.OrdinalIgnoreCase))
        {
            int decimalDigits = modeAndArgument.Length > 1 && int.TryParse(modeAndArgument[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedDigits)
                ? parsedDigits
                : 0;
            return TryToDecimal(value, out decimal percentValue) ? PersianNumberFormatter.FormatPercent(percentValue, decimalDigits) : null;
        }

        return TryToInt64(value, out long numberValue) ? PersianNumberFormatter.FormatNumber(numberValue) : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(PersianNumberConverter)} is one-way only (formatting happens at the binding edge, DESK-ARCH-14).");

    private static bool TryToInt64(object value, out long result)
    {
        try
        {
            result = System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
        {
            result = 0;
            return false;
        }
    }

    private static bool TryToDecimal(object value, out decimal result)
    {
        try
        {
            result = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
        {
            result = 0m;
            return false;
        }
    }
}
