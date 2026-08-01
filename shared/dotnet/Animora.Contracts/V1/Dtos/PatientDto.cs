namespace Animora.Contracts.V1.Dtos;

// TODO(P2): replace with the Kiota-generated type once the /api/v1/patients OpenAPI operation
// exists; like OwnerDto this hand-authored shape is the additive seam CONV-19/20 describes for
// pre-backend P1 (SH-04).

/// <summary>
/// The minimal wire shape phase 05's patient list and medical-file header bind to. Carries no
/// logic and no EF Core attributes (SH-04).
/// </summary>
/// <remarks>
/// The owner is referenced by id and never embedded: Owner and Patient are separate aggregates
/// (05-domain-model.md), and a patient belongs to exactly one owner within one tenant (DOM-03).
/// A screen that needs the owner's name fetches <see cref="OwnerDto"/> by that id rather than
/// reading a copy denormalized onto this shape, which would go stale the moment the owner is
/// renamed.
/// </remarks>
public sealed record PatientDto
{
    /// <summary>The patient's id.</summary>
    public required Guid Id { get; init; }

    /// <summary>The owning <see cref="OwnerDto.Id"/> (DOM-03).</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>The patient's name.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// The patient's species. A string rather than a <c>V1.Enums</c> member for now: the accepted
    /// set is this phase's documented decision, not a server-authoritative registry yet — see
    /// <c>Animora.SharedKernel.Validation.Clients.PatientValidator.AllowedSpecies</c>, which is
    /// what validates it. Promoting it to an enum here later is append-only (CONV-10/11, SH-03).
    /// </summary>
    public required string Species { get; init; }

    /// <summary>
    /// The patient's sex; validated against
    /// <c>Animora.SharedKernel.Validation.Clients.PatientValidator.AllowedSexes</c>.
    /// </summary>
    public required string Sex { get; init; }

    /// <summary>Optional breed within <see cref="Species"/>; see <c>IPatientInput.Breed</c>.</summary>
    public string? Breed { get; init; }

    /// <summary>The patient's birth date (UTC midnight), or <see langword="null"/> when unknown; see <c>IPatientInput.BirthDateUtc</c>.</summary>
    public DateTime? BirthDateUtc { get; init; }

    /// <summary>
    /// Whether <see cref="BirthDateUtc"/> was derived from a staff-entered approximate age rather
    /// than a precisely known date. A status/provenance flag, not part of <c>IPatientInput</c>'s
    /// validated surface — the same reason <c>StaffMember.IsActive</c> sits beside, not inside,
    /// <c>IStaffInput</c>.
    /// </summary>
    public bool IsBirthDateEstimated { get; init; }

    /// <summary>
    /// The patient's most recently recorded weight in kilograms, or <see langword="null"/> when
    /// never weighed; see <c>IPatientInput.WeightKg</c>. The weight-over-time trend chart is
    /// sourced from Visits' <c>BiometricReading</c> rows instead (05-domain-model.md), left as
    /// <c>TODO(P1-07)</c> on the medical-file summary screen rather than duplicated here.
    /// </summary>
    public decimal? WeightKg { get; init; }

    /// <summary>Whether the patient has been sterilized; a status flag, kept beside rather than inside <c>IPatientInput</c>.</summary>
    public bool IsSterilized { get; init; }

    /// <summary>Optional microchip identifier; see <c>IPatientInput.MicrochipId</c>.</summary>
    public string? MicrochipId { get; init; }

    /// <summary>When <see cref="MicrochipId"/> was implanted, in UTC; see <c>IPatientInput.MicrochipImplantedAtUtc</c>.</summary>
    public DateTime? MicrochipImplantedAtUtc { get; init; }

    /// <summary>Optional coat/plumage color description; see <c>IPatientInput.Color</c>.</summary>
    public string? Color { get; init; }

    /// <summary>Optional free-text behavior/temperament note; see <c>IPatientInput.Temperament</c>.</summary>
    public string? Temperament { get; init; }

    /// <summary>
    /// The patient's living environment; validated against
    /// <c>Animora.SharedKernel.Validation.Clients.PatientValidator.AllowedHousingTypes</c>.
    /// </summary>
    public string? HousingType { get; init; }

    /// <summary>Optional free-text diet/feeding-regimen note; see <c>IPatientInput.Diet</c>.</summary>
    public string? Diet { get; init; }

    /// <summary>Optional physical-file/label barcode value; see <c>IPatientInput.BarcodeValue</c>.</summary>
    public string? BarcodeValue { get; init; }

    /// <summary>Optional free-text surgical history known at intake; see <c>IPatientInput.SurgicalHistory</c>.</summary>
    public string? SurgicalHistory { get; init; }
}
