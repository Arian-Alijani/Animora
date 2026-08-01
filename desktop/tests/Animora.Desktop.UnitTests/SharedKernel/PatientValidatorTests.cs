using Animora.SharedKernel.Validation.Clients;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class PatientValidatorTests
{
    private readonly PatientValidator _validator = new();

    private sealed record PatientInput : IPatientInput
    {
        public Guid OwnerId { get; init; } = Guid.CreateVersion7();

        public string Name { get; init; } = "پشمک";

        public string Species { get; init; } = "Cat";

        public string Sex { get; init; } = "Female";

        public string? Breed { get; init; }

        public DateTime? BirthDateUtc { get; init; }

        public decimal? WeightKg { get; init; }

        public string? MicrochipId { get; init; }

        public DateTime? MicrochipImplantedAtUtc { get; init; }

        public string? Color { get; init; }

        public string? Temperament { get; init; }

        public string? HousingType { get; init; }

        public string? Diet { get; init; }

        public string? BarcodeValue { get; init; }

        public string? SurgicalHistory { get; init; }
    }

    public static TheoryData<string> Species => new([.. PatientValidator.AllowedSpecies]);

    public static TheoryData<string> Sexes => new([.. PatientValidator.AllowedSexes]);

    [Fact]
    public void A_patient_with_an_owner_name_species_and_sex_is_valid()
    {
        _validator.Validate(new PatientInput()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Owner_id_is_required()
    {
        // DOM-03: a patient belongs to exactly one owner. Whether that owner row exists is a
        // persistence-boundary question this validator deliberately does not ask (SH-05).
        ValidationResult result = _validator.Validate(new PatientInput { OwnerId = Guid.Empty });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.OwnerId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Name_is_required(string name)
    {
        ValidationResult result = _validator.Validate(new PatientInput { Name = name });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.Name));
    }

    [Fact]
    public void Name_is_capped_at_100_characters()
    {
        _validator.Validate(new PatientInput { Name = new string('پ', 100) }).IsValid.Should().BeTrue();
        _validator.Validate(new PatientInput { Name = new string('پ', 101) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Species))]
    public void Every_published_species_is_accepted(string species)
    {
        // Binds the rule to the list a phase 05 picker will bind to: a value added to
        // AllowedSpecies without a matching rule change cannot pass unnoticed (INV-02).
        _validator.Validate(new PatientInput { Species = species }).IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Sexes))]
    public void Every_published_sex_is_accepted(string sex)
    {
        _validator.Validate(new PatientInput { Sex = sex }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Horse")]
    [InlineData("dog")]   // the value set is case-sensitive: it is a stored code, not display text
    [InlineData("گربه")]  // Persian display text belongs at the UI edge (CONV-05)
    public void Species_outside_the_published_set_is_rejected(string species)
    {
        ValidationResult result = _validator.Validate(new PatientInput { Species = species });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.Species));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Neutered")]
    [InlineData("male")]
    public void Sex_outside_the_published_set_is_rejected(string sex)
    {
        ValidationResult result = _validator.Validate(new PatientInput { Sex = sex });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.Sex));
    }

    [Fact]
    public void The_published_value_sets_are_non_empty_and_free_of_duplicates()
    {
        // These two lists are the single source a picker and this rule share; a duplicate would
        // surface twice in the UI, and an empty list would make every patient invalid.
        PatientValidator.AllowedSpecies.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
        PatientValidator.AllowedSexes.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_broken_field_is_reported_in_one_pass()
    {
        var patient = new PatientInput
        {
            OwnerId = Guid.Empty,
            Name = "",
            Species = "Horse",
            Sex = "Neutered",
        };

        ValidationResult result = _validator.Validate(patient);

        result.FailedProperties().Should().BeEquivalentTo(
            nameof(IPatientInput.OwnerId),
            nameof(IPatientInput.Name),
            nameof(IPatientInput.Species),
            nameof(IPatientInput.Sex));
    }

    [Fact]
    public void Validation_runs_with_no_io()
    {
        ValidatorContract.ShouldRunWithoutIo(_validator, new PatientInput());
    }

    [Fact]
    public void Every_patient_input_property_is_covered_by_a_rule()
    {
        ValidatorContract.ShouldRuleOnEveryPropertyOf(_validator);
    }
}
