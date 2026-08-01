using System.Globalization;
using Animora.Desktop.UI.Controls;
using Avalonia.Data.Converters;

namespace Animora.Desktop.Modules.Clients.Views;

/// <summary>
/// One-way <see cref="IValueConverter"/> from <c>MedicalFileSummaryViewModel.IsSterilized</c> /
/// <c>Patient.IsSterilized</c> to the summary header's status chip content (design-reference.md §6
/// status-chip anatomy), mirroring <c>Modules.Identity.Views.DeviceStatusConverter</c>'s
/// mode-string shape. Select the mode with <c>ConverterParameter</c> (case-insensitive):
/// <c>Text</c> (default) yields the Persian label, <c>Variant</c> yields the
/// <see cref="AccentVariant"/> the chip binds its color to.
/// </summary>
public sealed class SterilizationStatusConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML.</summary>
    public static readonly SterilizationStatusConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isSterilized)
        {
            return null;
        }

        var mode = parameter as string ?? string.Empty;
        if (mode.Equals("variant", StringComparison.OrdinalIgnoreCase))
        {
            return isSterilized ? AccentVariant.Info : AccentVariant.Warning;
        }

        return isSterilized ? "عقیم‌شده" : "عقیم نشده";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(SterilizationStatusConverter)} is one-way only (DESK-ARCH-14).");
}
