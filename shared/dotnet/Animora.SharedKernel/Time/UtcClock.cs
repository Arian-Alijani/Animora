namespace Animora.SharedKernel.Time;

/// <summary>
/// UTC-typed accessors over the ambient <see cref="TimeProvider"/> (CONV-06), so handler and
/// domain code reads "now" without ever calling <see cref="DateTime.Now"/> or
/// <see cref="DateTime.UtcNow"/> directly.
/// </summary>
/// <remarks>
/// This is an extension surface over the BCL <see cref="TimeProvider"/> that phase 01/02 already
/// inject (e.g. <c>JalaliDateFormatter</c>, the test-only <c>FixedTimeProvider</c>), not a second
/// clock abstraction for the same concern (AG-14) — a parallel <c>IClock</c> interface would let
/// one code path be mocked in tests while another silently is not.
/// <para>
/// Jalali conversion is out of scope here on purpose: it happens only at the UI binding edge
/// (CONV-05), never in domain or handler code, so nothing in this type ever touches
/// <see cref="System.Globalization.PersianCalendar"/>.
/// </para>
/// </remarks>
public static class UtcClock
{
    /// <summary>The current instant, always <see cref="DateTimeKind.Utc"/> (CONV-04).</summary>
    public static DateTime UtcNow(this TimeProvider timeProvider) => timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>
    /// The current UTC calendar date (midnight, <see cref="DateTimeKind.Utc"/>) — never the
    /// caller's local date, which is a UI-edge concern (CONV-05) this type does not decide.
    /// </summary>
    public static DateTime UtcToday(this TimeProvider timeProvider) => timeProvider.UtcNow().Date;
}
