namespace Animora.Desktop.UnitTests.Localization;

/// <summary>
/// Deterministic <see cref="TimeProvider"/> for CONV-06 tests: fixes "now" and forces
/// <see cref="LocalTimeZone"/> to UTC so date-formatting assertions never depend on the machine
/// running the test.
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
