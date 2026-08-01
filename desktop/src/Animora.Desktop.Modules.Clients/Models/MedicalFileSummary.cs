namespace Animora.Desktop.Modules.Clients.Models;

/// <summary>
/// The header read model the medical-file summary screen renders, scoped to the Patient aggregate
/// (05-domain-model.md): <c>MedicalFile</c> is a header inside <see cref="Patient"/>, not a
/// separate aggregate with its own id, so this shape carries no id of its own beyond
/// <see cref="PatientId"/> (the phase 05 TODO header note; AG-14, INV-18). Storage units and UTC
/// instants only — Jalali stamps are applied at the binding edge, never here (INV-13, DESK-ARCH-14).
/// </summary>
/// <remarks>
/// The screen's links out — visit history (Visits, phase 07) and attachments (Files, phase 08) —
/// stay <c>TODO(P1-07)</c>/<c>TODO(P1-08)</c> markers on the ViewModel/View, not fields here: this
/// header carries only what <see cref="Patient"/> itself owns (DT-01, CM-06).
/// </remarks>
/// <param name="PatientId">The patient's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="PatientName">Mirrors <c>IPatientInput.Name</c>.</param>
/// <param name="OwnerId">
/// The owning <see cref="Owner"/>'s id; a patient belongs to exactly one owner (DOM-03).
/// </param>
/// <param name="OwnerDisplayName">
/// The owning owner's <see cref="Owner.FullName"/> at read time — the same read-side
/// denormalization <see cref="Patient.OwnerDisplayName"/> already carries, so the summary header
/// never needs a second fetch just to show whose patient it is.
/// </param>
/// <param name="Species">Mirrors <c>IPatientInput.Species</c>.</param>
/// <param name="Sex">Mirrors <c>IPatientInput.Sex</c>.</param>
/// <param name="Breed">Mirrors <c>IPatientInput.Breed</c>.</param>
/// <param name="BirthDateUtc">
/// Mirrors <c>IPatientInput.BirthDateUtc</c>; age is derived from this at the UI binding edge and
/// never stored (DESK-ARCH-14, phase 05 TODO item 3's documented answer).
/// </param>
/// <param name="IsBirthDateEstimated">Mirrors <c>Patient.IsBirthDateEstimated</c>.</param>
/// <param name="WeightKg">
/// Mirrors <c>IPatientInput.WeightKg</c> — the one current value; the weight-over-time chart is
/// Visits' <c>BiometricReading</c> series (05-domain-model.md), left as a <c>TODO(P1-07)</c> link
/// on the ViewModel rather than duplicated here.
/// </param>
/// <param name="IsSterilized">Mirrors <c>Patient.IsSterilized</c>.</param>
/// <param name="MicrochipId">Mirrors <c>IPatientInput.MicrochipId</c>.</param>
/// <param name="MicrochipImplantedAtUtc">Mirrors <c>IPatientInput.MicrochipImplantedAtUtc</c>.</param>
/// <param name="Color">Mirrors <c>IPatientInput.Color</c>.</param>
/// <param name="Temperament">Mirrors <c>IPatientInput.Temperament</c>.</param>
/// <param name="HousingType">Mirrors <c>IPatientInput.HousingType</c>.</param>
/// <param name="Diet">Mirrors <c>IPatientInput.Diet</c>.</param>
/// <param name="BarcodeValue">Mirrors <c>IPatientInput.BarcodeValue</c>.</param>
/// <param name="SurgicalHistory">Mirrors <c>IPatientInput.SurgicalHistory</c>.</param>
public sealed record MedicalFileSummary(
    Guid PatientId,
    string PatientName,
    Guid OwnerId,
    string OwnerDisplayName,
    string Species,
    string Sex,
    string? Breed,
    DateTime? BirthDateUtc,
    bool IsBirthDateEstimated,
    decimal? WeightKg,
    bool IsSterilized,
    string? MicrochipId,
    DateTime? MicrochipImplantedAtUtc,
    string? Color,
    string? Temperament,
    string? HousingType,
    string? Diet,
    string? BarcodeValue,
    string? SurgicalHistory);
