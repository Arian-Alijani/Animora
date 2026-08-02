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

    public static TheoryData<string> HousingTypes => new([.. PatientValidator.AllowedHousingTypes]);

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
        // These lists are the single source a picker and this rule share; a duplicate would
        // surface twice in the UI, and an empty list would make every patient invalid.
        PatientValidator.AllowedSpecies.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
        PatientValidator.AllowedSexes.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
        PatientValidator.AllowedHousingTypes.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Temperament_is_optional(string? temperament)
    {
        _validator.Validate(new PatientInput { Temperament = temperament }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Temperament_is_capped_at_300_characters()
    {
        _validator.Validate(new PatientInput { Temperament = new string('آ', 300) }).IsValid.Should().BeTrue();
        _validator.Validate(new PatientInput { Temperament = new string('آ', 301) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Housing_type_is_optional(string? housingType)
    {
        _validator.Validate(new PatientInput { HousingType = housingType }).IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(HousingTypes))]
    public void Every_published_housing_type_is_accepted(string housingType)
    {
        // Binds the rule to the same registry a phase 05 picker binds to (INV-02), the same
        // reasoning Every_published_species_is_accepted already applies to AllowedSpecies.
        _validator.Validate(new PatientInput { HousingType = housingType }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Yard")]
    [InlineData("apartment")] // case-sensitive: a stored code, not display text
    [InlineData("آپارتمان")]  // Persian display text belongs at the UI edge (CONV-05)
    public void Housing_type_outside_the_published_set_is_rejected(string housingType)
    {
        ValidationResult result = _validator.Validate(new PatientInput { HousingType = housingType });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.HousingType));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Diet_is_optional(string? diet)
    {
        _validator.Validate(new PatientInput { Diet = diet }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Diet_is_capped_at_1000_characters()
    {
        _validator.Validate(new PatientInput { Diet = new string('غ', 1000) }).IsValid.Should().BeTrue();
        _validator.Validate(new PatientInput { Diet = new string('غ', 1001) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Barcode_value_is_optional(string? barcodeValue)
    {
        _validator.Validate(new PatientInput { BarcodeValue = barcodeValue }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ABC-123")]
    [InlineData("0001")]
    [InlineData("A")]
    public void Barcode_value_accepts_letters_digits_and_hyphens(string barcodeValue)
    {
        _validator.Validate(new PatientInput { BarcodeValue = barcodeValue }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("بارکد")]           // non-ASCII characters
    [InlineData("ABC 123")]         // space
    [InlineData("ABC_123")]         // underscore, not a hyphen
    public void Barcode_value_is_rejected_when_it_is_not_letters_digits_or_hyphens(string barcodeValue)
    {
        ValidationResult result = _validator.Validate(new PatientInput { BarcodeValue = barcodeValue });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.BarcodeValue));
    }

    [Fact]
    public void Barcode_value_longer_than_64_characters_is_rejected()
    {
        ValidationResult result = _validator.Validate(new PatientInput { BarcodeValue = new string('A', 65) });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.BarcodeValue));
    }

    [Fact]
    public void Microchip_implanted_at_is_optional_when_no_microchip_id_is_recorded()
    {
        _validator.Validate(new PatientInput { MicrochipImplantedAtUtc = null }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Microchip_implanted_at_must_be_utc()
    {
        var localDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local);

        ValidationResult result = _validator.Validate(new PatientInput
        {
            MicrochipId = "CHIP-1",
            MicrochipImplantedAtUtc = localDate,
        });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.MicrochipImplantedAtUtc));
    }

    [Fact]
    public void Microchip_implanted_at_requires_a_microchip_id()
    {
        // A date with no chip on record would say "implanted" for a chip that was never entered —
        // the two fields must arrive together (mirrors OwnerValidator's own cross-field rules).
        ValidationResult result = _validator.Validate(new PatientInput
        {
            MicrochipId = null,
            MicrochipImplantedAtUtc = DateTime.UtcNow.Date,
        });

        result.FailedProperties().Should().Contain(nameof(IPatientInput.MicrochipImplantedAtUtc));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Surgical_history_is_optional(string? surgicalHistory)
    {
        _validator.Validate(new PatientInput { SurgicalHistory = surgicalHistory }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Surgical_history_is_capped_at_2000_characters()
    {
        _validator.Validate(new PatientInput { SurgicalHistory = new string('ج', 2000) }).IsValid.Should().BeTrue();
        _validator.Validate(new PatientInput { SurgicalHistory = new string('ج', 2001) }).IsValid.Should().BeFalse();
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
