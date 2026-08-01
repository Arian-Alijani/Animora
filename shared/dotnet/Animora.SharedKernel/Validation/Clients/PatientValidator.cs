using System.Text.RegularExpressions;
using FluentValidation;

namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// I/O-free structural rules for <see cref="IPatientInput"/> (SH-05, CONV-18).
/// </summary>
/// <remarks>
/// 05-domain-model fixes the Patient aggregate boundary but not a species/sex value set (AG-02);
/// the sets below are this phase's documented decision — see
/// <c>Roadmap/Desktop/phases/03-shared-kernel-primitives/TODO.md</c> item 9 — kept public so the
/// same list backs a desktop species/sex picker (phase 05) without a second copy (INV-02). The
/// housing-type set and the rest of this phase's own field additions (item 3) follow the same
/// pattern and the same reviewable-default spirit.
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

    /// <summary>
    /// The accepted living-environment values (phase 05 item 3's "آپارتمان و باغ و ..." example),
    /// the same append-only-registry spirit as <see cref="AllowedSpecies"/>: a small, reportable
    /// taxonomy rather than free text, because it is exactly the kind of value a later filter/report
    /// groups patients by.
    /// </summary>
    public static readonly IReadOnlyCollection<string> AllowedHousingTypes =
        new[] { "Apartment", "House", "Garden", "Farm", "Other" };

    // Longest weight ever plausible for a species this validator accepts (AllowedSpecies has no
    // "Horse"/"Livestock" entry); generous headroom for "Other" rather than a tight per-species cap.
    private const decimal MaximumWeightKg = 300m;

    private static readonly string SpeciesMessage = $"Species must be one of: {string.Join(", ", AllowedSpecies)}.";
    private static readonly string SexMessage = $"Sex must be one of: {string.Join(", ", AllowedSexes)}.";
    private static readonly string HousingTypeMessage =
        $"Housing type must be one of: {string.Join(", ", AllowedHousingTypes)}.";

    // A barcode's scanned charset: letters, digits and hyphens only, matching how most label
    // printers/scanners round-trip a code — mirrors OwnerValidator's own regex-plus-message shape.
    private static readonly Regex BarcodePattern = new("^[A-Za-z0-9-]{1,64}$", RegexOptions.Compiled);

    private const string BarcodeMessage = "Barcode must be 1-64 letters, digits or hyphens.";

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

        RuleFor(patient => patient.Breed)
            .MaximumLength(100)
            .When(patient => !string.IsNullOrEmpty(patient.Breed));

        RuleFor(patient => patient.BirthDateUtc)
            .Must(date => date is null || date.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Birth date must be UTC (CONV-04).");

        RuleFor(patient => patient.WeightKg)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than zero.")
            .LessThanOrEqualTo(MaximumWeightKg)
            .WithMessage($"Weight must not exceed {MaximumWeightKg:0} kg.")
            .When(patient => patient.WeightKg is not null);

        RuleFor(patient => patient.MicrochipId)
            .MaximumLength(40)
            .When(patient => !string.IsNullOrEmpty(patient.MicrochipId));

        RuleFor(patient => patient.MicrochipImplantedAtUtc)
            .Must(date => date is null || date.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Microchip implant date must be UTC (CONV-04).")
            .Must((patient, implantedAt) => implantedAt is null || !string.IsNullOrWhiteSpace(patient.MicrochipId))
            .WithMessage("Microchip implant date requires a microchip id.");

        RuleFor(patient => patient.Color)
            .MaximumLength(100)
            .When(patient => !string.IsNullOrEmpty(patient.Color));

        RuleFor(patient => patient.Temperament)
            .MaximumLength(300)
            .When(patient => !string.IsNullOrEmpty(patient.Temperament));

        RuleFor(patient => patient.HousingType)
            .Must(housingType => AllowedHousingTypes.Contains(housingType))
            .WithMessage(HousingTypeMessage)
            .When(patient => !string.IsNullOrEmpty(patient.HousingType));

        RuleFor(patient => patient.Diet)
            .MaximumLength(1000)
            .When(patient => !string.IsNullOrEmpty(patient.Diet));

        RuleFor(patient => patient.BarcodeValue)
            .Matches(BarcodePattern)
            .WithMessage(BarcodeMessage)
            .When(patient => !string.IsNullOrEmpty(patient.BarcodeValue));

        RuleFor(patient => patient.SurgicalHistory)
            .MaximumLength(2000)
            .When(patient => !string.IsNullOrEmpty(patient.SurgicalHistory));
    }
}
