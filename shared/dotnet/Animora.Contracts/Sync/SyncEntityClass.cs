namespace Animora.Contracts.Sync;

/// <summary>
/// The four conflict-resolution classes a synced entity picks exactly one of, fixed at design time
/// and never changed without a protocol version bump
/// (09-sync-architecture.md#sync-entity-classes, SYNC-R-01).
/// </summary>
/// <remarks>
/// Member names and meaning mirror 09-sync-architecture.md's table verbatim, so a reader who knows
/// that doc recognizes this type without translation:
/// <list type="bullet">
/// <item><see cref="MutableLWW"/> — field-group Last-Write-Wins by HLC (SYNC-R-17); see
/// <see cref="FieldGroup"/>.</item>
/// <item><see cref="AppendOnly"/> — no update ever accepted after create; conflicts are
/// structurally impossible (SYNC-R-18).</item>
/// <item><see cref="StateMachine"/> — LWW on non-status fields, status transitions validated
/// against the entity's own state machine at apply time (SYNC-R-19).</item>
/// <item><see cref="ReferenceOnly"/> — server-authoritative, pulled read-only by desktop, never
/// pushed.</item>
/// </list>
/// This phase (05) only declares <see cref="MutableLWW"/> entities (Owner, Patient, MedicalFile);
/// the other three members exist so later phases' declarations have a shared vocabulary to pick
/// from, not because this phase uses them.
/// </remarks>
public enum SyncEntityClass
{
    /// <summary>Field-group Last-Write-Wins by HLC (SYNC-R-17). Requires <see cref="FieldGroup"/>s
    /// (SYNC-R-03).</summary>
    MutableLWW,

    /// <summary>No update ever accepted after create (SYNC-R-18).</summary>
    AppendOnly,

    /// <summary>LWW on non-status fields; status transitions validated against the entity's state
    /// machine (SYNC-R-19).</summary>
    StateMachine,

    /// <summary>Server-authoritative, pulled read-only by desktop, never pushed.</summary>
    ReferenceOnly,
}
