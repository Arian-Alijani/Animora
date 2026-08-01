namespace Animora.Contracts.V1.Dtos;

// TODO(P2): replace with the Kiota-generated type once the /api/v1/patients OpenAPI operation
// exists; like OwnerDto this hand-authored shape is the additive seam CONV-19/20 describes for
// pre-backend P1 (SH-04).

/// <summary>
/// The minimal wire shape phase 05's patient list and medical-file header bind to. Carries no
/// logic and no EF Core attributes (SH-04).
/// </summary>
/// <remarks>
/// The owner is referenced by id and never embedded: Owner and Patient are separate aggregates
/// (05-domain-model.md), and a patient belongs to exactly one owner within one tenant (DOM-03).
/// A screen that needs the owner's name fetches <see cref="OwnerDto"/> by that id rather than
/// reading a copy denormalized onto this shape, which would go stale the moment the owner is
/// renamed.
/// </remarks>
public sealed record PatientDto
{
    /// <summary>The patient's id.</summary>
    public required Guid Id { get; init; }

    /// <summary>The owning <see cref="OwnerDto.Id"/> (DOM-03).</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>The patient's name.</summary>
    public required string Name { get; init; }

    /// <summary>
    /// The patient's species. A string rather than a <c>V1.Enums</c> member for now: the accepted
    /// set is this phase's documented decision, not a server-authoritative registry yet — see
    /// <c>Animora.SharedKernel.Validation.Clients.PatientValidator.AllowedSpecies</c>, which is
    /// what validates it. Promoting it to an enum here later is append-only (CONV-10/11, SH-03).
    /// </summary>
    public required string Species { get; init; }

    /// <summary>
    /// The patient's sex; validated against
    /// <c>Animora.SharedKernel.Validation.Clients.PatientValidator.AllowedSexes</c>.
    /// </summary>
    public required string Sex { get; init; }
}
