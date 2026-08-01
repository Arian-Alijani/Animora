namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// The tenant-RBAC permission-claim catalog, transcribed verbatim from
/// 10-security-and-access-control.md's grouped table (SEC-09). <c>PlatformAdmin</c> is excluded:
/// its claims are platform-level, not tenant RBAC.
/// </summary>
/// <remarks>
/// This is a module-owned transcription, not a duplication of a server-owned registry (INV-18):
/// nothing outside <c>Animora.Contracts</c> is a second source of truth for an entity or enum, and
/// this catalog is neither — it is Stage A's stand-in for seed data the server does not exist to
/// provide yet.
/// </remarks>
// TODO(P2): source this catalog from the server's permission-claim seed data instead of this
// transcription (SEC-09, INV-18).
public static class PermissionCatalog
{
    /// <summary>Every claim a tenant role may bundle, in the table's module order.</summary>
    public static IReadOnlyList<PermissionClaim> All { get; } =
    [
        new PermissionClaim("staff.manage", "Identity", "مدیریت کارکنان"),
        new PermissionClaim("roles.manage", "Identity", "مدیریت نقش‌ها"),

        new PermissionClaim("owners.read", "Clients", "مشاهده صاحبان حیوانات"),
        new PermissionClaim("owners.write", "Clients", "مدیریت صاحبان حیوانات"),
        new PermissionClaim("patients.read", "Clients", "مشاهده بیماران"),
        new PermissionClaim("patients.write", "Clients", "مدیریت بیماران"),
        new PermissionClaim("medical-file.read", "Clients", "مشاهده پرونده پزشکی"),

        new PermissionClaim("visits.write", "Visits", "ثبت و مدیریت ویزیت"),
        new PermissionClaim("lab-results.write", "Visits", "ثبت نتایج آزمایش"),
        new PermissionClaim("biometrics.write", "Visits", "ثبت اطلاعات زیستی"),

        new PermissionClaim("appointments.write", "Scheduling", "مدیریت نوبت‌ها"),
        new PermissionClaim("resources.manage", "Scheduling", "مدیریت منابع"),
        new PermissionClaim("calendar.read-all", "Scheduling", "مشاهده تقویم همه پرسنل"),

        new PermissionClaim("invoices.issue", "Finance", "صدور فاکتور"),
        new PermissionClaim("invoices.void", "Finance", "ابطال فاکتور"),
        new PermissionClaim("cheques.manage", "Finance", "مدیریت چک‌ها"),
        new PermissionClaim("cash-session.open", "Finance", "افتتاح صندوق"),
        new PermissionClaim("cash-session.close", "Finance", "تسویه صندوق"),
        new PermissionClaim("expenses.manage", "Finance", "مدیریت هزینه‌ها"),

        new PermissionClaim("reports.view-basic", "Reporting", "مشاهده گزارش‌های پایه"),
        new PermissionClaim("reports.view-advanced", "Reporting", "مشاهده گزارش‌های پیشرفته"),

        new PermissionClaim("subscription.manage", "Licensing", "مدیریت اشتراک"),
    ];

    // Ordinal: a claim key is an identifier, not user text (mirrors RoleValidator's duplicate-key
    // check), so lookups must not fold two differently-cased keys into one match.
    private static readonly HashSet<string> KeySet = new(All.Select(claim => claim.Key), StringComparer.Ordinal);

    /// <summary>Whether <paramref name="key"/> names a claim in this catalog.</summary>
    public static bool IsKnownKey(string key) => KeySet.Contains(key);
}
