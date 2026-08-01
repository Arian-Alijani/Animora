using System.Globalization;

namespace Animora.SharedKernel.Primitives;

/// <summary>
/// The tenant a row belongs to (DOM-01), wrapping a <c>UUIDv7</c> <see cref="Guid"/> (CONV-01) so a
/// tenant key can never be passed where an entity key is expected.
/// </summary>
/// <remarks>
/// Conversions to and from <see cref="Guid"/> are explicit on purpose: crossing into the raw key
/// type is a persistence/wire boundary, and an implicit hop would let a plain <see cref="Guid"/>
/// silently satisfy a tenant parameter. There is deliberately no ambient "current tenant" accessor
/// here — the value is resolved from the authenticated principal and flows through the request
/// (DOM-02), so a static one would be a second, unauthenticated source of truth.
/// <para>
/// <c>default(TenantId)</c> is <see cref="Empty"/>, which is never a valid tenant; callers that
/// accept one from outside check <see cref="IsEmpty"/> (or let a validator do it).
/// </para>
/// </remarks>
public readonly record struct TenantId
{
    private TenantId(Guid value) => Value = value;

    /// <summary>The unset value, equal to <c>default(TenantId)</c>; never a valid tenant (DOM-01).</summary>
    public static TenantId Empty => default;

    /// <summary>The underlying <c>UUIDv7</c> key.</summary>
    public Guid Value { get; }

    /// <summary>Whether this is the unset value.</summary>
    public bool IsEmpty => Value == Guid.Empty;

    /// <summary>Generates a new client-side <c>UUIDv7</c> tenant id (CONV-01/02).</summary>
    public static TenantId New() => new(Guid.CreateVersion7());

    /// <summary>Wraps an existing key read from storage or the wire; the caller vouches for its origin.</summary>
    public static TenantId FromGuid(Guid value) => new(value);

    /// <summary>Unwraps to the storage/wire key.</summary>
    public Guid ToGuid() => Value;

    /// <summary>Explicit conversion equivalent to <see cref="FromGuid"/>.</summary>
    public static explicit operator TenantId(Guid value) => FromGuid(value);

    /// <summary>Explicit conversion equivalent to <see cref="ToGuid"/>.</summary>
    public static explicit operator Guid(TenantId tenantId) => tenantId.Value;

    /// <summary>Renders the bare key, so a log line carries the value and not the wrapper's shape (CONV-21).</summary>
    public override string ToString() => Value.ToString("D", CultureInfo.InvariantCulture);
}
