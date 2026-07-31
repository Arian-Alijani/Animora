using Animora.Desktop.UI.Localization;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Localization;

public class MoneyFormatterTests
{
    private readonly MoneyFormatter _formatter = new(new PersianNumberFormatter());

    [Fact]
    public void FormatRials_groups_thousands_with_persian_digits_and_the_rial_suffix()
    {
        _formatter.FormatRials(864_500_000m).Should().Be("۸۶۴,۵۰۰,۰۰۰ ریال");
    }

    [Fact]
    public void FormatTomans_divides_by_ten_and_appends_the_toman_suffix()
    {
        // design-reference.md §7's own example figure: ۸۶,۴۵۰,۰۰۰ تومان.
        _formatter.FormatTomans(864_500_000m).Should().Be("۸۶,۴۵۰,۰۰۰ تومان");
    }

    [Theory]
    [InlineData(0, "۰ ریال")]
    [InlineData(-15000, "-۱۵,۰۰۰ ریال")]
    public void FormatRials_handles_zero_and_negative_amounts(decimal amount, string expected)
    {
        _formatter.FormatRials(amount).Should().Be(expected);
    }

    [Fact]
    public void FormatTomans_rounds_a_fractional_toman_remainder_to_the_nearest_whole_toman()
    {
        // 12,345 Rials / 10 = 1,234.5 Tomans; banker's rounding (FIN-20) rounds the .5 to the
        // nearest even whole number, i.e. 1,234.
        _formatter.FormatTomans(12_345m).Should().Be("۱,۲۳۴ تومان");
    }

    [Fact]
    public void FormatRials_ignores_the_decimal_places_ledger_amounts_are_persisted_with()
    {
        // FIN-19: decimal(18,2) storage, no fractional sub-unit in practice.
        _formatter.FormatRials(1500.00m).Should().Be("۱,۵۰۰ ریال");
    }
}
