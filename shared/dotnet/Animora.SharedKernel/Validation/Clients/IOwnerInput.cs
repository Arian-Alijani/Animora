namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// The property surface an owner create/edit command implements, so <see cref="OwnerValidator"/>
/// runs directly against the command (CONV-18, INV-02) instead of a copied input DTO.
/// </summary>
public interface IOwnerInput
{
    /// <summary>The owner's full name.</summary>
    string FullName { get; }

    /// <summary>
    /// Iranian mobile number, e.g. <c>"09121234567"</c> — required, since it is the channel
    /// appointment/reminder notifications go out on (see 14-jobs-and-notifications.md).
    /// </summary>
    string MobileNumber { get; }

    /// <summary>Optional Iranian landline number, digits only including the area code.</summary>
    string? LandlineNumber { get; }

    /// <summary>
    /// Optional 10-digit Iranian national ID ("کد ملی"); not every owner supplies one at intake.
    /// </summary>
    string? NationalId { get; }
}
