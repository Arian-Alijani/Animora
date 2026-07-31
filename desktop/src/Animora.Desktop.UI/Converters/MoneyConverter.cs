using System.Globalization;
using Animora.Desktop.UI.Localization;
using Avalonia.Data.Converters;

namespace Animora.Desktop.UI.Converters;

/// <summary>
/// One-way <see cref="IValueConverter"/> binding wrapper over <see cref="MoneyFormatter"/>
/// (CONV-07, INV-05). Holds no formatting logic of its own. Bind a <see langword="decimal"/>
/// amount already in Rials (the ledger's persisted unit, FIN-03) and select the unit with
/// <c>ConverterParameter</c> (case-insensitive): <c>Toman</c> (default, matches the reference
/// screens' displayed unit) or <c>Rial</c>.
/// </summary>
public sealed class MoneyConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML.</summary>
    public static readonly MoneyConverter Instance = new(new MoneyFormatter(new PersianNumberFormatter()));

    private readonly MoneyFormatter _formatter;

    public MoneyConverter(MoneyFormatter formatter)
    {
        _formatter = formatter;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        decimal amountInRials;
        try
        {
            amountInRials = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException)
        {
            return null;
        }

        string unit = parameter as string ?? string.Empty;
        return unit.Equals("rial", StringComparison.OrdinalIgnoreCase)
            ? _formatter.FormatRials(amountInRials)
            : _formatter.FormatTomans(amountInRials);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(MoneyConverter)} is one-way only (formatting happens at the binding edge, DESK-ARCH-14).");
}
