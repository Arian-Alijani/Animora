namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// One entry in the tenant-RBAC permission-claim catalog: a platform-defined capability a role can
/// bundle (SEC-09), grouped by the module that owns the capability it gates.
/// </summary>
/// <param name="Key">
/// The stable <c>{resource}.{action}</c> identifier (AG-12) a <c>Role</c> stores in
/// <see cref="Role.PermissionClaimKeys"/> and a <c>RoleValidator</c>-shaped command carries; never
/// shown to the user directly.
/// </param>
/// <param name="ModuleName">
/// The owning module's canonical catalog name (04-module-catalog.md), e.g. <c>"Identity"</c> or
/// <c>"Finance"</c> — what the role-management screen groups claims by.
/// </param>
/// <param name="DisplayName">The Persian label the role-management screen renders for this claim.</param>
public sealed record PermissionClaim(string Key, string ModuleName, string DisplayName);
