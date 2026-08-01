namespace Animora.SharedKernel.Primitives;

/// <summary>
/// The identity shape every persisted entity implements.
/// </summary>
/// <remarks>
/// Read-only by contract: an id is assigned once at construction — client-side for
/// desktop-originated rows, server-side otherwise (CONV-02) — and re-keying an existing row is
/// never a legal operation (CONV-03, AT-07). Implementations therefore expose no public setter and
/// keep the backing field private.
/// </remarks>
public interface IEntity
{
    /// <summary>The entity's <c>UUIDv7</c> primary key (INV-03, CONV-01).</summary>
    Guid Id { get; }
}
