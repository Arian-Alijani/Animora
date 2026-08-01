namespace Animora.Desktop.Modules.Identity.Models;

/// <summary>
/// The read-only row the device-listing screen binds to (SEC-06). Storage units and UTC instants
/// only — Jalali formatting is applied at the binding edge (INV-13, DESK-ARCH-14).
/// </summary>
/// <remarks>
/// The corpus fixes device-binding and seat mechanics (LIC-07/LIC-08) but not a listing screen's
/// columns; this field set is this phase's documented decision (AG-02), open to correction like the
/// staff/role/credential field rules items 1-3 recorded. Seat/limit enforcement and revocation are
/// server-side (LIC-08) and out of scope for this shape and this screen — the owner-admin manages
/// active devices from the web panel, not from here.
/// </remarks>
/// <param name="Id">The device record's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="DeviceName">A human-readable label for the row, e.g. the machine's host name.</param>
/// <param name="FingerprintSuffix">
/// The last few characters of the device's hardware fingerprint (LIC-07) — enough for an owner-admin
/// to tell two rows apart, never the full value, which stays a licensing-server concern.
/// </param>
/// <param name="RegisteredByStaffName">
/// The <see cref="StaffMember.FullName"/> of whoever was signed in when this device first
/// registered (SEC-06).
/// </param>
/// <param name="RegisteredAtUtc">When this device first registered.</param>
/// <param name="LastActiveAtUtc">When this device last completed a sync handshake.</param>
/// <param name="IsActive">
/// Whether the device's seat is currently active rather than revoked (LIC-08/LIC-09). Informational
/// only in this phase: no revoke action exists here, revocation happening on the web panel instead.
/// </param>
public sealed record DeviceRegistration(
    Guid Id,
    string DeviceName,
    string FingerprintSuffix,
    string RegisteredByStaffName,
    DateTime RegisteredAtUtc,
    DateTime LastActiveAtUtc,
    bool IsActive);
