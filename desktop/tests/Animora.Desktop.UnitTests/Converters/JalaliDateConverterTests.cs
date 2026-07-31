using System.Globalization;
using Animora.Desktop.UI.Converters;
using Animora.Desktop.UI.Localization;
using Animora.Desktop.UnitTests.Localization;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Converters;

public class JalaliDateConverterTests
{
    private static readonly DateTime Nowruz1403 = new(2024, 3, 20, 14, 5, 0, DateTimeKind.Utc);

    private static JalaliDateConverter CreateConverter() =>
        new(new JalaliDateFormatter(new FixedTimeProvider(Nowruz1403)));

    [Theory]
    [InlineData(null, "۱۴۰۳/۰۱/۰۱")]
    [InlineData("Date", "۱۴۰۳/۰۱/۰۱")]
    [InlineData("LongDate", "۱ فروردین ۱۴۰۳")]
    [InlineData("Time", "۱۴:۰۵")]
    [InlineData("DateTime", "۱۴۰۳/۰۱/۰۱ ۱۴:۰۵")]
    [InlineData("RelativeDay", "امروز")]
    public void Convert_dispatches_to_the_formatter_method_named_by_the_parameter(string? parameter, string expected)
    {
        object? result = CreateConverter().Convert(Nowruz1403, typeof(string), parameter, CultureInfo.InvariantCulture);

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_returns_null_for_a_non_datetime_value()
    {
        object? result = CreateConverter().Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertBack_is_not_supported()
    {
        var act = () => CreateConverter().ConvertBack("۱۴۰۳/۰۱/۰۱", typeof(DateTime), null, CultureInfo.InvariantCulture);

        act.Should().Throw<NotSupportedException>();
    }
}
