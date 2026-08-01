namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// The property surface an owner create/edit command implements, so <see cref="OwnerValidator"/>
/// runs directly against the command (CONV-18, INV-02) instead of a copied input DTO.
/// </summary>
/// <remarks>
/// <see cref="Address"/>, <see cref="City"/>, <see cref="Notes"/> and <see cref="IntakeDateUtc"/>
/// are phase 05's documented decision for "which fields does the owner form capture beyond these
/// four" (<c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> item 2), not an invented
/// default — reviewable/adjustable without touching any other layer the same way item 9's
/// phase-03 defaults already are.
/// </remarks>
public interface IOwnerInput
{
    /// <summary>The owner's full name.</summary>
    string FullName { get; }

    /// <summary>
    /// Iranian mobile number, e.g. <c>"09121234567"</c> — required, since it is the channel
    /// appointment/reminder notifications go out on (see 14-jobs-and-notifications.md).
    /// </summary>
    /// <remarks>
    /// Not unique across owners: two owners (e.g. members of one household) may share a mobile
    /// number, unlike a staff <c>Username</c> (SEC-03) — a phone number identifies a contact
    /// channel, not a sign-in identity, so nothing here rejects a duplicate the way
    /// <c>IdentityErrors.UsernameAlreadyTaken</c> does. Phase 05's documented answer to item 4.
    /// </remarks>
    string MobileNumber { get; }

    /// <summary>Optional Iranian landline number, digits only including the area code.</summary>
    string? LandlineNumber { get; }

    /// <summary>
    /// Optional 10-digit Iranian national ID ("کد ملی"); not every owner supplies one at intake.
    /// </summary>
    string? NationalId { get; }

    /// <summary>Optional street address.</summary>
    string? Address { get; }

    /// <summary>Optional city name.</summary>
    string? City { get; }

    /// <summary>
    /// Optional clinic-internal note about the owner (e.g. billing arrangements, preferred
    /// contact times) — never shown to the owner themselves, the same non-owner-facing spirit as
    /// <c>Error.Detail</c>'s diagnostic-only text.
    /// </summary>
    string? Notes { get; }

    /// <summary>
    /// When this owner's file was opened, in UTC (CONV-04) — the "زمان ایجاد پرونده" the clinic
    /// records for every owner. A real business field the form pre-fills with today's date and
    /// lets staff edit (e.g. entering a client days after the actual first visit), not a
    /// system-assigned audit stamp the way an entity's own <c>CreatedAtUtc</c> would be.
    /// </summary>
    DateTime IntakeDateUtc { get; }
}
