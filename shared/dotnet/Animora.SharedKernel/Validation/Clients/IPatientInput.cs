namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// The property surface a patient (animal) create/edit command implements, validated directly by
/// <see cref="PatientValidator"/> (CONV-18) instead of a copied input DTO.
/// </summary>
/// <remarks>
/// <see cref="Breed"/> through <see cref="SurgicalHistory"/> are phase 05's documented decision
/// for "which fields does the patient form capture beyond these four"
/// (<c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> item 3), not an invented
/// default. Two related flags are deliberately absent from this surface: <c>IsSterilized</c> and
/// an "estimated birth date" flag are status/provenance markers, not judged by structural
/// validation — the same reason <c>IStaffInput</c> omits <c>IsActive</c> — so they travel on the
/// concrete save command and the read model (<c>PatientDto</c>) instead, beside but not inside
/// this interface.
/// </remarks>
public interface IPatientInput
{
    /// <summary>
    /// The owning <c>Owner</c>'s id; a patient belongs to exactly one owner (DOM-03). Whether that
    /// owner actually exists is a persistence-boundary lookup, which this I/O-free validator
    /// deliberately does not perform (SH-05) — only "not the empty id" is checked here.
    /// </summary>
    Guid OwnerId { get; }

    /// <summary>The patient's name.</summary>
    string Name { get; }

    /// <summary>The patient's species, from <see cref="PatientValidator.AllowedSpecies"/>.</summary>
    string Species { get; }

    /// <summary>The patient's sex, from <see cref="PatientValidator.AllowedSexes"/>.</summary>
    string Sex { get; }

    /// <summary>Optional breed within <see cref="Species"/>; free text, no registry yet.</summary>
    string? Breed { get; }

    /// <summary>
    /// The patient's birth date (date-only, stored at UTC midnight, CONV-04), or
    /// <see langword="null"/> when unknown. The single stored source of truth for age: age itself
    /// is never persisted, only computed at the UI binding edge from this value (DESK-ARCH-14),
    /// so the "birth date or age, whichever is known" intake flow the form supports (an estimated
    /// age converted to an estimated birth date before dispatch) never creates two numbers that
    /// can drift apart.
    /// </summary>
    DateTime? BirthDateUtc { get; }

    /// <summary>
    /// The patient's most recently recorded weight, in kilograms, or <see langword="null"/> when
    /// never weighed. This is the one current value the intake/edit form itself captures; the
    /// weight-over-time trend the medical file summary charts is a series of
    /// <c>BiometricReading</c> rows, a separate Visits-owned aggregate (05-domain-model.md) this
    /// phase's screens link out to as <c>TODO(P1-07)</c> rather than duplicate here.
    /// </summary>
    decimal? WeightKg { get; }

    /// <summary>
    /// Optional microchip identifier as printed/scanned off the chip. Free text rather than a
    /// digits-only pattern: chip standards and reader output vary by manufacturer and region, the
    /// same reasoning <c>OwnerValidator</c> already applies to not pinning a mobile carrier
    /// prefix.
    /// </summary>
    string? MicrochipId { get; }

    /// <summary>
    /// When <see cref="MicrochipId"/> was implanted, in UTC (CONV-04), or <see langword="null"/>
    /// when no chip is recorded yet.
    /// </summary>
    DateTime? MicrochipImplantedAtUtc { get; }

    /// <summary>Optional coat/plumage color description; free text.</summary>
    string? Color { get; }

    /// <summary>
    /// Optional free-text behavior/temperament note (e.g. calm, anxious around strangers) — kept
    /// as prose rather than a closed set like <see cref="Species"/>/<see cref="Sex"/>, because a
    /// temperament description loses information once forced into a short fixed list.
    /// </summary>
    string? Temperament { get; }

    /// <summary>The patient's living environment, from <see cref="PatientValidator.AllowedHousingTypes"/>.</summary>
    string? HousingType { get; }

    /// <summary>Optional free-text diet/feeding-regimen note.</summary>
    string? Diet { get; }

    /// <summary>
    /// Optional barcode value printed on the patient's physical file/label for front-desk lookup —
    /// distinct from <see cref="MicrochipId"/>, which identifies the chip implanted in the animal
    /// itself, not a paper record.
    /// </summary>
    string? BarcodeValue { get; }

    /// <summary>
    /// Optional free-text summary of surgical history known at intake (e.g. from a previous
    /// clinic). Surgeries performed at this clinic going forward are recorded as Visits-owned
    /// visit outcomes (05-domain-model.md), not by editing this field.
    /// </summary>
    string? SurgicalHistory { get; }
}
