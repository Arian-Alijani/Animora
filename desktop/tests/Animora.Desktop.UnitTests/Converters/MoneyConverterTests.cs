using System.Globalization;
using Animora.Desktop.UI.Converters;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Converters;

public class MoneyConverterTests
{
    private readonly MoneyConverter _converter = new();

    [Fact]
    public void Convert_defaults_to_toman_formatting()
    {
        object? result = _converter.Convert(864_500_000m, typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().Be("۸۶,۴۵۰,۰۰۰ تومان");
    }

    [Fact]
    public void Convert_with_Rial_parameter_formats_the_raw_rial_amount()
    {
        object? result = _converter.Convert(864_500_000m, typeof(string), "Rial", CultureInfo.InvariantCulture);

        result.Should().Be("۸۶۴,۵۰۰,۰۰۰ ریال");
    }

    [Fact]
    public void Convert_returns_null_for_a_null_value()
    {
        object? result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void Convert_returns_null_for_a_non_numeric_value()
    {
        object? result = _converter.Convert("not a number", typeof(string), null, CultureInfo.InvariantCulture);

        result.Should().BeNull();
    }

    [Fact]
    public void ConvertBack_is_not_supported()
    {
        var act = () => _converter.ConvertBack("۸۶,۴۵۰,۰۰۰ تومان", typeof(decimal), null, CultureInfo.InvariantCulture);

        act.Should().Throw<NotSupportedException>();
    }
}
