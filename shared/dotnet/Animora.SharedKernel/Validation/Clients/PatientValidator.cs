using FluentValidation;

namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// I/O-free structural rules for <see cref="IPatientInput"/> (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// 05-domain-model fixes the Patient aggregate boundary but not a species/sex value set (AG-02);
/// the sets below are this phase's documented decision — see
/// <c>Roadmap/Desktop/phases/03-shared-kernel-primitives/TODO.md</c> item 9 — kept public so the
/// same list backs a desktop species/sex picker (phase 05) without a second copy (INV-02).
/// </remarks>
public sealed class PatientValidator : AbstractValidator<IPatientInput>
{
    /// <summary>
    /// The accepted species at intake. Extend by adding a value here, never by accepting free
    /// text — this list is the enum registry's spirit (append-only, SH-03) applied before a real
    /// <c>Contracts</c> enum for species is needed by a later module phase.
    /// </summary>
    public static readonly IReadOnlyCollection<string> AllowedSpecies =
        new[] { "Dog", "Cat", "Bird", "Rabbit", "Rodent", "Reptile", "Other" };

    /// <summary>The accepted sex values; "Unknown" covers intake before an exam confirms sex.</summary>
    public static readonly IReadOnlyCollection<string> AllowedSexes = new[] { "Male", "Female", "Unknown" };

    private static readonly string SpeciesMessage = $"Species must be one of: {string.Join(", ", AllowedSpecies)}.";
    private static readonly string SexMessage = $"Sex must be one of: {string.Join(", ", AllowedSexes)}.";

    public PatientValidator()
    {
        RuleFor(patient => patient.OwnerId)
            .NotEqual(Guid.Empty);

        RuleFor(patient => patient.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(patient => patient.Species)
            .NotEmpty()
            .Must(species => AllowedSpecies.Contains(species))
            .WithMessage(SpeciesMessage);

        RuleFor(patient => patient.Sex)
            .NotEmpty()
            .Must(sex => AllowedSexes.Contains(sex))
            .WithMessage(SexMessage);
    }
}
