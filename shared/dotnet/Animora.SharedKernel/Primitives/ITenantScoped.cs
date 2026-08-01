namespace Animora.SharedKernel.Primitives;

/// <summary>
/// Implemented by every entity that belongs to a tenant (DOM-01) — that is, everything except
/// platform-level records such as <c>Plan</c> or the global permission-claim catalog.
/// </summary>
/// <remarks>
/// Separate from <see cref="IEntity"/> so tenancy is an explicit, greppable decision per entity
/// rather than an inherited default, and so query filters and the persistence boundary can select
/// exactly the tenant-scoped set. The value is assigned from the authenticated context, never from
/// a request body (DOM-02), and never changes afterwards — a row does not move between tenants
/// (DOM-03).
/// </remarks>
public interface ITenantScoped
{
    /// <summary>The owning tenant; never <see cref="TenantId.Empty"/> on a persisted row.</summary>
    TenantId TenantId { get; }
}
