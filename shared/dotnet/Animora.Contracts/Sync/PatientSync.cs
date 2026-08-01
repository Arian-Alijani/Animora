namespace Animora.Contracts.Sync;

/// <summary>
/// Patient's sync classification and field groups — the one step of the "add a synced entity"
/// recipe this phase (05) can honor ahead of phase 16's local-data/outbox work
/// (<c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> item 30, playbook "add a synced
/// entity" step 2, SYNC-R-01/03). Field names match
/// <c>Animora.Desktop.Modules.Clients.Models.Patient</c> / <c>IPatientInput</c> (phase 05 TODO item
/// 3's answer); <c>Id</c> and the read-time-only <c>OwnerDisplayName</c> projection carry no group
/// — the former is never LWW-resolved, the latter is not a stored field at all (resolved by a join,
/// <c>Patient.OwnerDisplayName</c>'s own doc comment).
/// </summary>
public static class PatientSync
{
    /// <summary>Patient is <see cref="SyncEntityClass.MutableLWW"/> per
    /// 09-sync-architecture.md's own table.</summary>
    public const SyncEntityClass Class = SyncEntityClass.MutableLWW;

    /// <summary>
    /// Five groups, mirroring 09-sync-architecture.md's own illustrative split for this entity
    /// (<c>identity{name,species,breed}</c>, ...) extended to every field phase 05 item 3 actually
    /// added, grouped by which fields staff realistically edit in one sitting (SYNC-R-03).
    /// </summary>
    public static readonly IReadOnlyList<FieldGroup> FieldGroups =
    [
        // What the animal fundamentally is: set at intake, revised together on the rare correction.
        new FieldGroup("identity", ["Name", "Species", "Sex", "Breed", "Color"]),

        // Physical facts that can legitimately change on a later visit, independent of identity
        // above. WeightKg here is only ever the one current value (IPatientInput.WeightKg's own
        // doc comment) — the historical series is Visits' AppendOnly-classed BiometricReading, a
        // separate entity phase 07 declares its own sync class for, not a field group here.
        new FieldGroup("biometrics", ["BirthDateUtc", "IsBirthDateEstimated", "WeightKg", "IsSterilized"]),

        // A microchip is implanted once and rarely re-recorded; grouping its id with its own implant
        // date keeps that one edit event from being entangled with the biometrics group above.
        new FieldGroup("microchip", ["MicrochipId", "MicrochipImplantedAtUtc"]),

        // Free-text clinical/behavioral notes a clinician updates together during an intake
        // interview or a follow-up conversation with the owner.
        new FieldGroup("clinicalProfile", ["Temperament", "HousingType", "Diet", "SurgicalHistory"]),

        // Administrative: which owner this patient belongs to (DOM-03) and the physical file's own
        // barcode label — both set once and rarely revisited, but for reasons unrelated to the
        // clinical/biometric groups above, hence their own group.
        new FieldGroup("administrative", ["OwnerId", "BarcodeValue"]),
    ];
}
