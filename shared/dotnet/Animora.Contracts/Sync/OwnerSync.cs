namespace Animora.Contracts.Sync;

/// <summary>
/// Owner's sync classification and field groups — the one step of the "add a synced entity" recipe
/// this phase (05) can honor ahead of phase 16's local-data/outbox work
/// (<c>Roadmap/Desktop/phases/05-clients-module-screens/TODO.md</c> item 30, playbook "add a synced
/// entity" step 2, SYNC-R-01/03). Field names match <c>Animora.Desktop.Modules.Clients.Models.Owner</c>
/// / <c>IOwnerInput</c> (phase 05 TODO item 2's answer); <c>Id</c> itself carries no group since a
/// row's key is never LWW-resolved, only its fields are.
/// </summary>
public static class OwnerSync
{
    /// <summary>Owner is <see cref="SyncEntityClass.MutableLWW"/> per 09-sync-architecture.md's own
    /// table, which names Owner alongside Patient/Appointment/MedicalFile header as examples.</summary>
    public const SyncEntityClass Class = SyncEntityClass.MutableLWW;

    /// <summary>
    /// Four groups, split by how independently the underlying data actually changes in practice:
    /// front-desk correcting a misspelled name rarely happens in the same edit as updating an
    /// address, and the clinic-internal note is edited by different staff, at different times, than
    /// either — each deserves its own HLC so those edits never race each other for no reason
    /// (SYNC-R-03).
    /// </summary>
    public static readonly IReadOnlyList<FieldGroup> FieldGroups =
    [
        // Who this owner is, as a person: rarely revised once entered, and revised together when it is.
        new FieldGroup("identity", ["FullName", "NationalId"]),

        // How the clinic reaches/finds this owner: the fields staff actually update after a move or
        // a new phone number, independent of the identity group above.
        new FieldGroup("contact", ["MobileNumber", "LandlineNumber", "Address", "City"]),

        // Clinic-internal only (IOwnerInput.Notes' own doc comment) — never owner-facing, and edited
        // on its own cadence (e.g. a billing note added mid-relationship).
        new FieldGroup("clinicNotes", ["Notes"]),

        // Administrative provenance: when the file was opened, set once at intake and only rarely
        // corrected afterward (phase 05 TODO item 2's answer).
        new FieldGroup("intake", ["IntakeDateUtc"]),
    ];
}
