using System.Globalization;
using Animora.Desktop.UI.Converters;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Converters;

public class PersianNumberConverterTests
{
    private readonly PersianNumberConverter _converter = new();

    [Theory]
    [InlineData(null, 86450000, "۸۶,۴۵۰,۰۰۰")]
    [InlineData("Number", 1284, "۱,۲۸۴")]
    public void Convert_defaults_to_grouped_number_formatting(string? parameter, long value, string expected)
    {
        object? result = _converter.Convert(value, typeof(string), parameter, CultureInfo.InvariantCulture);

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_with_Percent_parameter_formats_a_percentage()
    {
        object? result = _converter.Convert(12m, typeof(string), "Percent", CultureInfo.InvariantCulture);

        result.Should().Be("۱۲%");
    }

    [Fact]
    public void Convert_with_Percent_and_a_decimal_digit_count_formats_accordingly()
    {
        object? result = _converter.Convert(12.5m, typeof(string), "Percent:1", CultureInfo.InvariantCulture);

        result.Should().Be("۱۲.۵%");
    }

    [Fact]
    public void Convert_with_Digits_parameter_only_swaps_digits_without_grouping()
    {
        object? result = _converter.Convert("v2.0", typeof(string), "Digits", CultureInfo.InvariantCulture);

        result.Should().Be("v۲.۰");
    }

    [Fact]
    public void Convert_returns_null_for_a_null_value()
    {
        object? result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void Convert_returns_null_when_the_number_mode_receives_a_non_numeric_value()
    {
        object? result = _converter.Convert("abc", typeof(string), "Number", CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertBack_is_not_supported()
    {
        var act = () => _converter.ConvertBack("۱,۲۸۴", typeof(long), null, CultureInfo.InvariantCulture);

        act.Should().Throw<NotSupportedException>();
    }
}
