using Animora.Desktop.Modules.Clients.Models;
using Animora.SharedKernel.Validation.Clients;

namespace Animora.Desktop.Modules.Clients.Data;

// TODO(P1-16): delete this type and rebind IPatientReadStore/IPatientWriteStore to the
// Dapper-backed reader and EF Core-backed writer over the local database (DT-05, INV-20). Nothing
// but the two registration lines in Composition/ServiceCollectionExtensions changes with it
// (DIR-03).
/// <summary>
/// Satisfies both <see cref="IPatientReadStore"/> and <see cref="IPatientWriteStore"/> over
/// <see cref="ClientsSampleData"/>, so a create made through the write half shows up in the read
/// half's next query the way one SQLite table would (DIR-03, DT-12).
/// </summary>
internal sealed class InMemoryPatientStore : IPatientReadStore, IPatientWriteStore
{
    private readonly ClientsSampleData _sampleData;

    public InMemoryPatientStore(ClientsSampleData sampleData)
    {
        _sampleData = sampleData;
    }

    public Task<PatientPage> GetPageAsync(
        Guid? ownerId,
        string? searchTerm,
        string? afterId,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            IEnumerable<PatientRecord> filtered = _sampleData.Patients;

            // The optional owner filter is what lets one method serve both the global and the
            // owner-scoped list mode (this seam's own IPatientReadStore.GetPageAsync doc comment;
            // AG-14, DESK-ARCH-05, CONV-17).
            if (ownerId is { } scopedOwnerId)
            {
                filtered = filtered.Where(patient => patient.OwnerId == scopedOwnerId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filtered = filtered.Where(patient =>
                    patient.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (patient.Breed is not null &&
                        patient.Breed.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (patient.MicrochipId is not null &&
                        patient.MicrochipId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (patient.BarcodeValue is not null &&
                        patient.BarcodeValue.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(afterId) && Guid.TryParse(afterId, out var afterPatientId))
            {
                filtered = filtered.Where(patient => patient.Id.CompareTo(afterPatientId) > 0);
            }

            // Patient.Id, for the same reason IOwnerReadStore.GetPageAsync orders by Owner.Id
            // instead of Name/MicrochipId (CONV-16, DT-08).
            var ordered = filtered.OrderBy(patient => patient.Id);

            // One extra row than the page size, so "was there another page" never needs a second
            // count query — exactly what Stage C's Dapper reader will do with LIMIT (limit + 1).
            var window = ordered.Take(limit + 1).ToList();
            var hasMore = window.Count > limit;
            var items = window.Take(limit).Select(ToPatient).ToList();
            var nextCursor = hasMore ? items[^1].Id.ToString() : null;

            return Task.FromResult(new PatientPage(items, nextCursor));
        }
    }

    public Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var record = _sampleData.Patients.FirstOrDefault(candidate => candidate.Id == patientId);
            return Task.FromResult(record is null ? null : ToPatient(record));
        }
    }

    public Task<MedicalFileSummary?> GetMedicalFileSummaryAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            var record = _sampleData.Patients.FirstOrDefault(candidate => candidate.Id == patientId);
            return Task.FromResult(record is null ? null : ToMedicalFileSummary(record));
        }
    }

    public Task SaveAsync(
        Guid patientId,
        IPatientInput input,
        bool isBirthDateEstimated,
        bool isSterilized,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sampleData.Gate)
        {
            _sampleData.Patients.RemoveAll(patient => patient.Id == patientId);
            _sampleData.Patients.Add(new PatientRecord(
                patientId,
                input.OwnerId,
                input.Name,
                input.Species,
                input.Sex,
                input.Breed,
                input.BirthDateUtc,
                isBirthDateEstimated,
                input.WeightKg,
                isSterilized,
                input.MicrochipId,
                input.MicrochipImplantedAtUtc,
                input.Color,
                input.Temperament,
                input.HousingType,
                input.Diet,
                input.BarcodeValue,
                input.SurgicalHistory));

            return Task.CompletedTask;
        }
    }

    // Resolves Owner.FullName at read time (must run inside the caller's Gate lock): the join
    // Stage C's Dapper reader performs with one query (DT-05), Patient.OwnerDisplayName's own doc
    // comment.
    private Patient ToPatient(PatientRecord record)
    {
        return new Patient(
            record.Id,
            record.OwnerId,
            ResolveOwnerDisplayName(record.OwnerId),
            record.Name,
            record.Species,
            record.Sex,
            record.Breed,
            record.BirthDateUtc,
            record.IsBirthDateEstimated,
            record.WeightKg,
            record.IsSterilized,
            record.MicrochipId,
            record.MicrochipImplantedAtUtc,
            record.Color,
            record.Temperament,
            record.HousingType,
            record.Diet,
            record.BarcodeValue,
            record.SurgicalHistory);
    }

    // Reads through the same PatientRecord row Patient itself is projected from — the phase 05
    // TODO header's "medical-file summary reads through the patient seam" decision applied to this
    // Stage A store (AG-14, INV-18) — rather than a second seeded collection.
    private MedicalFileSummary ToMedicalFileSummary(PatientRecord record)
    {
        return new MedicalFileSummary(
            record.Id,
            record.Name,
            record.OwnerId,
            ResolveOwnerDisplayName(record.OwnerId),
            record.Species,
            record.Sex,
            record.Breed,
            record.BirthDateUtc,
            record.IsBirthDateEstimated,
            record.WeightKg,
            record.IsSterilized,
            record.MicrochipId,
            record.MicrochipImplantedAtUtc,
            record.Color,
            record.Temperament,
            record.HousingType,
            record.Diet,
            record.BarcodeValue,
            record.SurgicalHistory);
    }

    private string ResolveOwnerDisplayName(Guid ownerId) =>
        _sampleData.Owners.FirstOrDefault(owner => owner.Id == ownerId)?.FullName ?? string.Empty;
}
