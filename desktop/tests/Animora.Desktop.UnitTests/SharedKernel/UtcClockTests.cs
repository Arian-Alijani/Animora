using Animora.Desktop.UnitTests.Localization;
using Animora.SharedKernel.Time;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class UtcClockTests
{
    // Late enough in the UTC day that any accidental local-time conversion (Tehran is UTC+03:30)
    // would land on the following calendar date and fail the UtcToday assertions below.
    private static readonly DateTimeOffset LateEvening = new(2024, 3, 20, 22, 45, 30, TimeSpan.Zero);

    private static FixedTimeProvider FixedAt(DateTimeOffset instant) => new(instant);

    [Fact]
    public void UtcNow_returns_the_providers_instant()
    {
        FixedAt(LateEvening).UtcNow().Should().Be(LateEvening.UtcDateTime);
    }

    [Fact]
    public void UtcNow_is_always_kind_utc()
    {
        // CONV-04: a DateTime leaving this surface with Unspecified kind is what silently becomes
        // a local timestamp three layers later.
        FixedAt(LateEvening).UtcNow().Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void UtcNow_reads_an_offset_instant_as_its_utc_equivalent()
    {
        var tehranMidnight = new DateTimeOffset(2024, 3, 21, 0, 0, 0, TimeSpan.FromMinutes(210));

        FixedAt(tehranMidnight).UtcNow().Should().Be(new DateTime(2024, 3, 20, 20, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void UtcNow_is_deterministic_under_a_fixed_provider()
    {
        FixedTimeProvider timeProvider = FixedAt(LateEvening);

        // CONV-06's point: nothing reachable from here calls DateTime.UtcNow, so a test that pins
        // the provider pins every timestamp the code under test produces.
        timeProvider.UtcNow().Should().Be(timeProvider.UtcNow());
    }

    [Fact]
    public void UtcToday_is_midnight_of_the_utc_date()
    {
        FixedAt(LateEvening).UtcToday().Should().Be(new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void UtcToday_is_always_kind_utc()
    {
        FixedAt(LateEvening).UtcToday().Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void UtcToday_uses_the_utc_date_and_not_the_local_one()
    {
        // 2024-03-20T21:00Z is already 2024-03-21 in Tehran. UtcToday must still report the 20th:
        // the local calendar day is a UI-edge decision (CONV-05), not this type's.
        var beforeMidnightUtc = new DateTimeOffset(2024, 3, 20, 21, 0, 0, TimeSpan.Zero);

        FixedAt(beforeMidnightUtc).UtcToday().Should().Be(new DateTime(2024, 3, 20, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void UtcToday_agrees_with_the_date_component_of_UtcNow()
    {
        FixedTimeProvider timeProvider = FixedAt(LateEvening);

        timeProvider.UtcToday().Should().Be(timeProvider.UtcNow().Date);
    }
}
