namespace Animora.SharedKernel.Validation.Identity;

/// <summary>
/// The property surface a role create/edit command implements, so <see cref="RoleValidator"/> runs
/// directly against the command (CONV-18, INV-02).
/// </summary>
/// <remarks>
/// A role is a tenant-named bundle of platform-defined claims (SEC-09), which is why the claims
/// arrive as keys: the catalog those keys are drawn from is owned by the Identity module (and, from
/// P2, by the server's seed data), not by this assembly.
/// </remarks>
public interface IRoleInput
{
    /// <summary>The tenant-visible Persian role name, e.g. <c>"پذیرش"</c>.</summary>
    string DisplayName { get; }

    /// <summary>
    /// The permission-claim keys this role bundles, in <c>{resource}.{action}</c> form (AG-12).
    /// </summary>
    IReadOnlyCollection<string> PermissionClaimKeys { get; }
}
