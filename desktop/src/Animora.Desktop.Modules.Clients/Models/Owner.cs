namespace Animora.Desktop.Modules.Clients.Models;

/// <summary>
/// The owner list-row and the owner create/edit form's read model. Storage units and UTC instants
/// only — Persian digit/date formatting is applied at the binding edge, never here (INV-13,
/// DESK-ARCH-14).
/// </summary>
/// <param name="Id">The owner's <c>UUIDv7</c> id (INV-03).</param>
/// <param name="FullName">Mirrors <c>IOwnerInput.FullName</c>.</param>
/// <param name="MobileNumber">
/// Mirrors <c>IOwnerInput.MobileNumber</c>. Not unique across owners — two owners (e.g. members of
/// one household) may share one — per phase 05 TODO item 4's documented answer.
/// </param>
/// <param name="LandlineNumber">Mirrors <c>IOwnerInput.LandlineNumber</c>.</param>
/// <param name="NationalId">Mirrors <c>IOwnerInput.NationalId</c>.</param>
/// <param name="Address">Mirrors <c>IOwnerInput.Address</c>.</param>
/// <param name="City">Mirrors <c>IOwnerInput.City</c>.</param>
/// <param name="Notes">
/// Mirrors <c>IOwnerInput.Notes</c> — clinic-internal, never shown to the owner.
/// </param>
/// <param name="IntakeDateUtc">Mirrors <c>IOwnerInput.IntakeDateUtc</c>.</param>
public sealed record Owner(
    Guid Id,
    string FullName,
    string MobileNumber,
    string? LandlineNumber,
    string? NationalId,
    string? Address,
    string? City,
    string? Notes,
    DateTime IntakeDateUtc);
