using System.Globalization;
using Animora.Desktop.UI.Controls;
using Avalonia.Data.Converters;

namespace Animora.Desktop.Modules.Identity.Views;

/// <summary>
/// One-way <see cref="IValueConverter"/> from <see cref="Models.DeviceRegistration.IsActive"/> to the
/// device list's status chip content (design-reference.md §6 status-chip anatomy), mirroring
/// <see cref="StaffStatusConverter"/>'s mode-string shape. A separate converter rather than reusing
/// <see cref="StaffStatusConverter"/> directly: a device's <c>false</c> state means "revoked"
/// (LIC-08/LIC-09), a different Persian label than a staff account's "غیرفعال". Select the mode with
/// <c>ConverterParameter</c> (case-insensitive): <c>Text</c> (default) yields the Persian label,
/// <c>Variant</c> yields the <see cref="AccentVariant"/> the chip binds its color to.
/// </summary>
public sealed class DeviceStatusConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML.</summary>
    public static readonly DeviceStatusConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isActive)
        {
            return null;
        }

        var mode = parameter as string ?? string.Empty;
        if (mode.Equals("variant", StringComparison.OrdinalIgnoreCase))
        {
            return isActive ? AccentVariant.Success : AccentVariant.Danger;
        }

        return isActive ? "فعال" : "لغو شده";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(DeviceStatusConverter)} is one-way only (DESK-ARCH-14).");
}
