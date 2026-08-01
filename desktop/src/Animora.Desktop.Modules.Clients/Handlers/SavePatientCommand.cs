using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Clients;
using Mediator;

namespace Animora.Desktop.Modules.Clients.Handlers;

/// <summary>
/// The patient form's one dispatch target for both create and edit (playbook step 3). Implements
/// <see cref="IPatientInput"/> directly (CONV-18, INV-02) so <see cref="PatientValidator"/> runs
/// against the command itself, mirroring <see cref="SaveOwnerCommand"/>'s shape.
/// </summary>
/// <param name="PatientId">
/// <see langword="null"/> for a create, in which case the handler assigns a fresh <c>UUIDv7</c>
/// (INV-03); the row being edited otherwise.
/// </param>
/// <param name="OwnerId">
/// Mirrors <see cref="IPatientInput.OwnerId"/>. Existence is not this validator's concern (SH-05);
/// <see cref="SavePatientHandler"/> checks it against <c>IOwnerReadStore.ExistsAsync</c> and returns
/// <c>ClientsErrors.OwnerNotFound</c> when it fails (DOM-03).
/// </param>
/// <param name="Name">Mirrors <see cref="IPatientInput.Name"/>.</param>
/// <param name="Species">Mirrors <see cref="IPatientInput.Species"/>.</param>
/// <param name="Sex">Mirrors <see cref="IPatientInput.Sex"/>.</param>
/// <param name="Breed">Mirrors <see cref="IPatientInput.Breed"/>.</param>
/// <param name="BirthDateUtc">Mirrors <see cref="IPatientInput.BirthDateUtc"/>.</param>
/// <param name="WeightKg">Mirrors <see cref="IPatientInput.WeightKg"/>.</param>
/// <param name="MicrochipId">Mirrors <see cref="IPatientInput.MicrochipId"/>.</param>
/// <param name="MicrochipImplantedAtUtc">Mirrors <see cref="IPatientInput.MicrochipImplantedAtUtc"/>.</param>
/// <param name="Color">Mirrors <see cref="IPatientInput.Color"/>.</param>
/// <param name="Temperament">Mirrors <see cref="IPatientInput.Temperament"/>.</param>
/// <param name="HousingType">Mirrors <see cref="IPatientInput.HousingType"/>.</param>
/// <param name="Diet">Mirrors <see cref="IPatientInput.Diet"/>.</param>
/// <param name="BarcodeValue">Mirrors <see cref="IPatientInput.BarcodeValue"/>.</param>
/// <param name="SurgicalHistory">Mirrors <see cref="IPatientInput.SurgicalHistory"/>.</param>
/// <param name="IsBirthDateEstimated">
/// Whether <see cref="BirthDateUtc"/> was derived from a staff-entered approximate age rather than
/// a precisely known date — a provenance flag, not part of <see cref="IPatientInput"/>'s validated
/// surface (phase 05 TODO item 3's documented answer), carried straight through to
/// <c>IPatientWriteStore.SaveAsync</c>.
/// </param>
/// <param name="IsSterilized">
/// Whether the patient has been sterilized; a status flag kept beside, not inside,
/// <see cref="IPatientInput"/> for the same reason as <paramref name="IsBirthDateEstimated"/>,
/// mirroring <c>SaveStaffMemberCommand.IsActive</c>.
/// </param>
public sealed record SavePatientCommand(
    Guid? PatientId,
    Guid OwnerId,
    string Name,
    string Species,
    string Sex,
    string? Breed,
    DateTime? BirthDateUtc,
    decimal? WeightKg,
    string? MicrochipId,
    DateTime? MicrochipImplantedAtUtc,
    string? Color,
    string? Temperament,
    string? HousingType,
    string? Diet,
    string? BarcodeValue,
    string? SurgicalHistory,
    bool IsBirthDateEstimated,
    bool IsSterilized) : IPatientInput, ICommand<Result<Guid>>;
