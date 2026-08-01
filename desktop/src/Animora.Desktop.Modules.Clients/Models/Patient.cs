namespace Animora.Desktop.Modules.Clients.Models;

/// <summary>
/// The patient list-row and the patient create/edit form's read model. Storage units and UTC
/// instants only — age is derived from <see cref="BirthDateUtc"/> at the UI binding edge and never
/// stored (INV-13, DESK-ARCH-14, phase 05 TODO item 3's documented answer).
/// </summary>
/// <param name="Id">The patient's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="OwnerId">
/// The owning <see cref="Owner"/>'s id; a patient belongs to exactly one owner (DOM-03).
/// </param>
/// <param name="OwnerDisplayName">
/// The owning owner's <see cref="Owner.FullName"/> at read time. A read-side denormalization, not a
/// cross-aggregate embed — this shape is the module's own hot-read projection (Stage C resolves it
/// with one joined Dapper query, DT-05), the same reasoning <c>StaffMember.RoleDisplayName</c>
/// already documents — so carrying it here is what spares the virtualized patient list, in both its
/// global and owner-scoped modes, a per-row round trip it would otherwise need (DT-08).
/// </param>
/// <param name="Name">Mirrors <c>IPatientInput.Name</c>.</param>
/// <param name="Species">Mirrors <c>IPatientInput.Species</c>.</param>
/// <param name="Sex">Mirrors <c>IPatientInput.Sex</c>.</param>
/// <param name="Breed">Mirrors <c>IPatientInput.Breed</c>.</param>
/// <param name="BirthDateUtc">Mirrors <c>IPatientInput.BirthDateUtc</c>.</param>
/// <param name="IsBirthDateEstimated">
/// Whether <see cref="BirthDateUtc"/> was derived from a staff-entered approximate age rather than
/// a precisely known date. A status/provenance flag, not part of <c>IPatientInput</c>'s validated
/// surface — mirrors <c>Animora.Contracts.V1.Dtos.PatientDto.IsBirthDateEstimated</c> and the same
/// reason <c>StaffMember.IsActive</c> sits beside, not inside, <c>IStaffInput</c>.
/// </param>
/// <param name="WeightKg">Mirrors <c>IPatientInput.WeightKg</c>.</param>
/// <param name="IsSterilized">
/// Whether the patient has been sterilized; a status flag kept beside, not inside,
/// <c>IPatientInput</c> for the same reason as <see cref="IsBirthDateEstimated"/>.
/// </param>
/// <param name="MicrochipId">Mirrors <c>IPatientInput.MicrochipId</c>.</param>
/// <param name="MicrochipImplantedAtUtc">Mirrors <c>IPatientInput.MicrochipImplantedAtUtc</c>.</param>
/// <param name="Color">Mirrors <c>IPatientInput.Color</c>.</param>
/// <param name="Temperament">Mirrors <c>IPatientInput.Temperament</c>.</param>
/// <param name="HousingType">Mirrors <c>IPatientInput.HousingType</c>.</param>
/// <param name="Diet">Mirrors <c>IPatientInput.Diet</c>.</param>
/// <param name="BarcodeValue">Mirrors <c>IPatientInput.BarcodeValue</c>.</param>
/// <param name="SurgicalHistory">Mirrors <c>IPatientInput.SurgicalHistory</c>.</param>
public sealed record Patient(
    Guid Id,
    Guid OwnerId,
    string OwnerDisplayName,
    string Name,
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
