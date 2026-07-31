using Animora.Desktop.UI.Localization;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Localization;

public class PersianNumberFormatterTests
{
    private readonly PersianNumberFormatter _formatter = new();

    [Theory]
    [InlineData("0123456789", "۰۱۲۳۴۵۶۷۸۹")]
    [InlineData("۱۴۰۳/۰۱/۰۱", "۱۴۰۳/۰۱/۰۱")]
    [InlineData("v2.0", "v۲.۰")]
    public void ToPersianDigits_replaces_only_ascii_digits(string input, string expected)
    {
        _formatter.ToPersianDigits(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1284, "۱,۲۸۴")]
    [InlineData(86450000, "۸۶,۴۵۰,۰۰۰")]
    [InlineData(0, "۰")]
    [InlineData(-1500, "-۱,۵۰۰")]
    public void FormatNumber_groups_thousands_with_persian_digits(long value, string expected)
    {
        _formatter.FormatNumber(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(12, 0, "۱۲%")]
    [InlineData(12.5, 1, "۱۲.۵%")]
    [InlineData(0, 0, "۰%")]
    public void FormatPercent_appends_ascii_percent_sign(decimal value, int decimalDigits, string expected)
    {
        _formatter.FormatPercent(value, decimalDigits).Should().Be(expected);
    }
}
