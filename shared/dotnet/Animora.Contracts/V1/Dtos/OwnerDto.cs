namespace Animora.Contracts.V1.Dtos;

// TODO(P2): replace with the Kiota-generated type once the /api/v1/owners OpenAPI operation
// exists; this hand-authored shape is the additive seam CONV-19/20 describes for pre-backend P1
// (SH-04) — its field names/types are chosen so that swap is additive, not a rewrite.

/// <summary>
/// The minimal wire shape phase 05's owner screens bind to: list rows and the create/edit form's
/// read model. Carries no logic and no EF Core attributes (SH-04).
/// </summary>
public sealed record OwnerDto
{
    /// <summary>The owner's id.</summary>
    public required Guid Id { get; init; }

    /// <summary>The owner's full name.</summary>
    public required string FullName { get; init; }

    /// <summary>Iranian mobile number; see <c>Animora.SharedKernel.Validation.Clients.IOwnerInput</c>
    /// for the format this value satisfies.</summary>
    public required string MobileNumber { get; init; }

    /// <summary>Optional Iranian landline number.</summary>
    public string? LandlineNumber { get; init; }

    /// <summary>Optional 10-digit Iranian national ID.</summary>
    public string? NationalId { get; init; }
}
