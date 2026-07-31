using Animora.Desktop.UI.Localization;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Localization;

public class JalaliDateFormatterTests
{
    // Nowruz 1403 (1 Farvardin 1403) fell on 2024-03-20; LocalTimeZone is fixed to UTC in
    // FixedTimeProvider, so the hour component here is arbitrary and does not cross a day boundary.
    private static readonly DateTime Nowruz1403 = new(2024, 3, 20, 10, 0, 0, DateTimeKind.Utc);

    private static JalaliDateFormatter CreateFormatter(DateTimeOffset? now = null) =>
        new(new FixedTimeProvider(now ?? DateTimeOffset.UtcNow));

    [Fact]
    public void FormatDate_converts_to_persian_digit_jalali_date()
    {
        CreateFormatter().FormatDate(Nowruz1403).Should().Be("۱۴۰۳/۰۱/۰۱");
    }

    [Fact]
    public void FormatLongDate_includes_the_persian_month_name()
    {
        CreateFormatter().FormatLongDate(Nowruz1403).Should().Be("۱ فروردین ۱۴۰۳");
    }

    [Fact]
    public void FormatTime_uses_24_hour_persian_digits()
    {
        var utc = new DateTime(2024, 3, 20, 14, 5, 0, DateTimeKind.Utc);

        CreateFormatter().FormatTime(utc).Should().Be("۱۴:۰۵");
    }

    [Fact]
    public void FormatDateTime_combines_date_and_time()
    {
        var utc = new DateTime(2024, 3, 20, 14, 5, 0, DateTimeKind.Utc);

        CreateFormatter().FormatDateTime(utc).Should().Be("۱۴۰۳/۰۱/۰۱ ۱۴:۰۵");
    }

    [Theory]
    [InlineData(0, "امروز")]
    [InlineData(-1, "دیروز")]
    [InlineData(1, "فردا")]
    public void FormatRelativeDay_labels_the_three_days_around_now(int offsetDays, string expected)
    {
        var now = new DateTimeOffset(2024, 3, 20, 8, 0, 0, TimeSpan.Zero);
        var formatter = CreateFormatter(now);

        formatter.FormatRelativeDay(now.UtcDateTime.AddDays(offsetDays)).Should().Be(expected);
    }

    [Fact]
    public void FormatRelativeDay_falls_back_to_FormatDate_outside_the_three_day_window()
    {
        var now = new DateTimeOffset(2024, 3, 20, 8, 0, 0, TimeSpan.Zero);
        var formatter = CreateFormatter(now);
        var value = now.UtcDateTime.AddDays(5);

        formatter.FormatRelativeDay(value).Should().Be(formatter.FormatDate(value));
    }

    [Fact]
    public void FormatDate_throws_for_non_utc_input()
    {
        var formatter = CreateFormatter();
        var unspecified = DateTime.SpecifyKind(Nowruz1403, DateTimeKind.Unspecified);

        var act = () => formatter.FormatDate(unspecified);

        act.Should().Throw<ArgumentException>();
    }
}
