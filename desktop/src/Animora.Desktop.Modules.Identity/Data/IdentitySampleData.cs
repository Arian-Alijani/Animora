using Animora.Desktop.Modules.Identity.Models;

namespace Animora.Desktop.Modules.Identity.Data;

/// <summary>One seeded staff account, before the read-time join that produces a <c>StaffMember</c>.</summary>
internal sealed record StaffAccount(
    Guid Id,
    string FullName,
    string Username,
    string MobileNumber,
    string? Email,
    Guid RoleId,
    bool IsActive);

/// <summary>One seeded role, before the read-time join that produces a <c>Role</c>'s <c>MemberCount</c>.</summary>
internal sealed record RoleDefinition(
    Guid Id,
    string DisplayName,
    IReadOnlyCollection<string> PermissionClaimKeys,
    bool IsSystemRole);

// TODO(P1-15): delete this type once phase 15 (Identity Local Data) ships the SQLite-backed reads
// and writes; nothing but the four Stage A bindings in Composition/ServiceCollectionExtensions
// changes with it (DIR-03).
/// <summary>
/// The one seeded Persian demo dataset every Stage A fake in this folder reads and writes against
/// (items 16-18): a tenant with an owner-admin, two other roles, four staff accounts and their
/// credentials, and a handful of registered devices.
/// </summary>
/// <remarks>
/// A singleton shared by every Stage A store rather than four private copies: <c>InMemoryStaffStore</c>
/// and <c>InMemoryRoleStore</c> each need to read the *other* half of this data to build their
/// read model's denormalized fields (<c>StaffMember.RoleDisplayName</c>, <c>Role.MemberCount</c>) —
/// exactly the join Stage C resolves with one Dapper query (DT-05) — so a shared instance is what
/// keeps a create on one store instantly visible to the other's projection, the way one SQLite
/// database would.
/// <para>
/// The seeded usernames follow SEC-17: <c>petshop</c> is the bare owner-admin username, and every
/// other seeded account is prefixed with it (<c>petshop-drahmadi</c>, <c>petshop-zsadeghi</c>,
/// <c>petshop-hkarimi</c>) — the same example the corpus uses in
/// 10-security-and-access-control.md's SEC-17 entry.
/// </para>
/// </remarks>
internal sealed class IdentitySampleData
{
    // Every seeded credential shares one password: a real per-account value would only look
    // meaningful, and this seam is deleted whole once phase 15 lands (DT-12).
    private const string SeededPassword = "Petshop@123";

    private readonly List<StaffAccount> _staff;
    private readonly List<RoleDefinition> _roles;
    private readonly List<DeviceRegistration> _devices;
    private readonly Dictionary<Guid, string> _passwordsByStaffId;

    public IdentitySampleData()
    {
        var ownerAdminRoleId = Guid.CreateVersion7();
        var veterinarianRoleId = Guid.CreateVersion7();
        var receptionistRoleId = Guid.CreateVersion7();

        _roles =
        [
            new RoleDefinition(
                ownerAdminRoleId,
                "مدیر کلینیک",
                // The owner-admin bundles every claim in the catalog (SEC-11): it is the tenant's
                // one always-fully-privileged role, not a role an owner-admin could under-grant.
                PermissionCatalog.All.Select(claim => claim.Key).ToArray(),
                IsSystemRole: true),
            new RoleDefinition(
                veterinarianRoleId,
                "دامپزشک",
                [
                    "owners.read",
                    "patients.read",
                    "patients.write",
                    "medical-file.read",
                    "visits.write",
                    "lab-results.write",
                    "biometrics.write",
                    "calendar.read-all",
                    "reports.view-basic",
                ],
                IsSystemRole: false),
            new RoleDefinition(
                receptionistRoleId,
                "پذیرش",
                [
                    "owners.read",
                    "owners.write",
                    "patients.read",
                    "appointments.write",
                    "invoices.issue",
                    "cash-session.open",
                    "cash-session.close",
                ],
                IsSystemRole: false),
        ];

        var ownerAdminStaffId = Guid.CreateVersion7();
        var veterinarianStaffId = Guid.CreateVersion7();
        var receptionistStaffId = Guid.CreateVersion7();
        var inactiveStaffId = Guid.CreateVersion7();

        _staff =
        [
            new StaffAccount(
                ownerAdminStaffId,
                "امیر رحیمی",
                "petshop",
                "09121234567",
                "amir.rahimi@petshop.example",
                ownerAdminRoleId,
                IsActive: true),
            new StaffAccount(
                veterinarianStaffId,
                "دکتر سارا احمدی",
                "petshop-drahmadi",
                "09123456789",
                "sara.ahmadi@petshop.example",
                veterinarianRoleId,
                IsActive: true),
            new StaffAccount(
                receptionistStaffId,
                "زهرا صادقی",
                "petshop-zsadeghi",
                "09131234567",
                null,
                receptionistRoleId,
                IsActive: true),
            new StaffAccount(
                inactiveStaffId,
                "حسین کریمی",
                "petshop-hkarimi",
                "09191234567",
                null,
                veterinarianRoleId,
                // Deactivated on purpose: the login screen's account-inactive branch (SignInHandler,
                // IdentityErrors.AccountInactive) has nothing to click through against without one.
                IsActive: false),
        ];

        _passwordsByStaffId = new Dictionary<Guid, string>
        {
            [ownerAdminStaffId] = SeededPassword,
            [veterinarianStaffId] = SeededPassword,
            [receptionistStaffId] = SeededPassword,
            [inactiveStaffId] = SeededPassword,
        };

        _devices =
        [
            new DeviceRegistration(
                Guid.CreateVersion7(),
                "ایستگاه پذیرش",
                "A93F",
                "امیر رحیمی",
                RegisteredAtUtc: new DateTime(2026, 3, 10, 7, 30, 0, DateTimeKind.Utc),
                LastActiveAtUtc: new DateTime(2026, 7, 31, 16, 45, 0, DateTimeKind.Utc),
                IsActive: true),
            new DeviceRegistration(
                Guid.CreateVersion7(),
                "اتاق دامپزشک",
                "7C1E",
                "دکتر سارا احمدی",
                RegisteredAtUtc: new DateTime(2026, 4, 2, 9, 0, 0, DateTimeKind.Utc),
                LastActiveAtUtc: new DateTime(2026, 7, 30, 12, 10, 0, DateTimeKind.Utc),
                IsActive: true),
            new DeviceRegistration(
                Guid.CreateVersion7(),
                "لپ‌تاپ پشتیبان",
                "4B02",
                "امیر رحیمی",
                RegisteredAtUtc: new DateTime(2025, 11, 18, 10, 15, 0, DateTimeKind.Utc),
                LastActiveAtUtc: new DateTime(2026, 5, 6, 8, 20, 0, DateTimeKind.Utc),
                // Revoked: shows the listing screen's read-only inactive state (LIC-08/LIC-09).
                IsActive: false),
        ];
    }

    /// <summary>
    /// The one lock every Stage A store in this folder guards its reads and writes with. A single
    /// gate rather than one per collection: <see cref="InMemoryStaffStore"/> and
    /// <see cref="InMemoryRoleStore"/> each read across both <see cref="Staff"/> and
    /// <see cref="Roles"/> to build their read model's joined fields, so two separate locks could
    /// deadlock on cross-acquisition order.
    /// </summary>
    public object Gate { get; } = new();

    public List<StaffAccount> Staff => _staff;

    public List<RoleDefinition> Roles => _roles;

    public IReadOnlyList<DeviceRegistration> Devices => _devices;

    public IReadOnlyDictionary<Guid, string> PasswordsByStaffId => _passwordsByStaffId;
}
