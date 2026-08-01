using System.Globalization;
using Animora.Desktop.UI.Controls;
using Avalonia.Data.Converters;

namespace Animora.Desktop.Modules.Identity.Views;

/// <summary>
/// One-way <see cref="IValueConverter"/> from <see cref="Models.StaffMember.IsActive"/> to the staff
/// list's status chip content (design-reference.md §6 status-chip anatomy), mirroring
/// <see cref="Animora.Desktop.UI.Converters.PersianNumberConverter"/>'s mode-string shape. Select the
/// mode with <c>ConverterParameter</c> (case-insensitive): <c>Text</c> (default) yields the Persian
/// label, <c>Variant</c> yields the <see cref="AccentVariant"/> the chip binds its color to.
/// </summary>
public sealed class StaffStatusConverter : IValueConverter
{
    /// <summary>Shared instance for `{x:Static}` usage in XAML.</summary>
    public static readonly StaffStatusConverter Instance = new();

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

        return isActive ? "فعال" : "غیرفعال";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException($"{nameof(StaffStatusConverter)} is one-way only (DESK-ARCH-14).");
}
