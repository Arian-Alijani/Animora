namespace Animora.SharedKernel.Primitives;

/// <summary>
/// Marks an entity that participates in device/server synchronisation and therefore may only be
/// soft-deleted (INV-04): a delete flips <see cref="IsDeleted"/> and stamps
/// <see cref="DeletedAtUtc"/> so the removal itself is a change that can propagate.
/// </summary>
/// <remarks>
/// The marker deliberately stops at the tombstone shape. HLC stamps, field-group metadata and the
/// sync class of an entity are transport-side concerns owned by <c>SharedKernel/Sync</c> and
/// <c>Contracts/Sync</c>, and are added when sync itself exists — putting them here would force
/// every P1 entity to carry members nothing reads yet.
/// <para>
/// Implementing this marker is also what the persistence boundary tests key on: a synced entity
/// exposes no public setter for <see cref="IEntity.Id"/> (AT-07), because a re-keyed row is
/// indistinguishable from a new one after it has been replicated.
/// </para>
/// </remarks>
public interface ISyncedEntity : IEntity
{
    /// <summary>Whether this row is a tombstone; hard delete is forbidden on synced tables (INV-04).</summary>
    bool IsDeleted { get; }

    /// <summary>
    /// When the row was tombstoned, in UTC (CONV-04), or <see langword="null"/> while it is live.
    /// Drives the server-side tombstone retention window (SYNC-R-22).
    /// </summary>
    DateTime? DeletedAtUtc { get; }
}
