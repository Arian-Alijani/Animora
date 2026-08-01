namespace Animora.Contracts.V1.Enums;

/// <summary>
/// The Appointment lifecycle state (05-domain-model.md's Appointment state machine), as the
/// explicit shared numeric value the wire and every client switch on (CONV-10) — never a
/// localized string; Persian labels are resolved from the stable member name at the UI edge
/// (CONV-12).
/// </summary>
/// <remarks>
/// Values are append-only (CONV-11, SH-03): a retired value is marked
/// <see cref="ObsoleteAttribute"/> in this registry and never renumbered or reused, so a client
/// built against an older version never misreads one status as another.
/// </remarks>
public enum AppointmentStatus
{
    /// <summary>Booked but not yet confirmed by the clinic.</summary>
    Requested = 0,

    /// <summary>Confirmed by the clinic; the slot is held.</summary>
    Confirmed = 1,

    /// <summary>The owner/patient has arrived and checked in.</summary>
    CheckedIn = 2,

    /// <summary>The visit tied to this appointment has concluded.</summary>
    Completed = 3,

    /// <summary>
    /// Time/resource changed; the same appointment id is kept rather than creating a new row
    /// (05-domain-model.md), so this state is transient on the way back to <see cref="Confirmed"/>.
    /// </summary>
    Rescheduled = 4,

    /// <summary>Cancelled before the appointment took place.</summary>
    Cancelled = 5,

    /// <summary>
    /// Confirmed but the owner/patient never arrived; triggers the follow-up alert source
    /// (14-jobs-and-notifications.md).
    /// </summary>
    NoShow = 6,
}
