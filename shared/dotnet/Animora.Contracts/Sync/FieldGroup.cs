namespace Animora.Contracts.Sync;

/// <summary>
/// One named, independently-HLC-versioned subset of a <see cref="SyncEntityClass.MutableLWW"/>
/// entity's fields (09-sync-architecture.md#change-capture, SYNC-R-03): two devices editing
/// different groups of the same record both win instead of one clobbering the other, because each
/// group carries its own HLC stamp rather than the whole row sharing one.
/// </summary>
/// <param name="Name">
/// Stable, camelCase identifier for this group (e.g. <c>"identity"</c>, <c>"contact"</c>) — it
/// travels on the wire as the unit conflict resolution operates over, so renaming it is a breaking
/// protocol change (SYNC-R-01), not a free refactor.
/// </param>
/// <param name="FieldNames">
/// The entity's own property names captured by this group, exactly as declared on the desktop read
/// model / contract DTO (no wire-casing translation here). Every field the entity carries belongs
/// to exactly one group — a field with no natural group of its own still needs one, since an
/// ungrouped field would have no HLC to version by.
/// </param>
public sealed record FieldGroup(string Name, IReadOnlyList<string> FieldNames);
