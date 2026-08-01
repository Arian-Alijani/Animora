namespace Animora.Contracts.Sync;

/// <summary>
/// MedicalFile's sync classification and field groups — the one step of the "add a synced entity"
/// recipe this phase (05) can honor ahead of phase 16's local-data/outbox work
/// (<c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> item 30, playbook "add a synced
/// entity" step 2, SYNC-R-01/03).
/// </summary>
/// <remarks>
/// 09-sync-architecture.md's own sync-entity-class table lists "MedicalFile header" as a
/// <see cref="SyncEntityClass.MutableLWW"/> example beside Owner/Patient/Appointment, so this
/// declaration exists to name that entry — but <c>MedicalFile</c> is a header living inside the
/// Patient aggregate, not a row of its own (05-domain-model.md, this phase's TODO header's own "the
/// medical-file summary reads through the patient seam, not a seam of its own" decision, AG-14,
/// INV-18; see also <c>Models.MedicalFileSummary</c>'s and <c>IPatientReadStore.GetMedicalFileSummaryAsync</c>'s
/// doc comments for the same call applied one layer down). Its sync unit is therefore the same
/// Patient row <see cref="PatientSync"/> already declares, and its groups are a reference to that
/// declaration's clinical subset rather than a duplicate list a future edit could drift out of sync
/// with (INV-02) — changing a field's group membership happens in exactly one place either way.
/// </remarks>
public static class MedicalFileSync
{
    /// <summary>MedicalFile is <see cref="SyncEntityClass.MutableLWW"/>, inherited from the
    /// <see cref="PatientSync"/> row it is a header view over.</summary>
    public const SyncEntityClass Class = SyncEntityClass.MutableLWW;

    /// <summary>
    /// The clinical-header subset of <see cref="PatientSync.FieldGroups"/> the medical-file summary
    /// screen renders (item 27) — <c>identity</c> and <c>administrative</c> are excluded because
    /// they describe the patient record itself rather than its medical-file header (the same split
    /// <c>Models.MedicalFileSummary</c> and <c>Models.Patient</c> already draw independently).
    /// </summary>
    public static readonly IReadOnlyList<FieldGroup> FieldGroups =
    [
        PatientSync.FieldGroups.Single(group => group.Name == "biometrics"),
        PatientSync.FieldGroups.Single(group => group.Name == "microchip"),
        PatientSync.FieldGroups.Single(group => group.Name == "clinicalProfile"),
    ];
}
