using Animora.SharedKernel.Validation.Clients;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class OwnerValidatorTests
{
    private readonly OwnerValidator _validator = new();

    // The command shapes phase 05 will send in; the record stands in for them so the rules are
    // exercised through IOwnerInput exactly as a handler's command will be (CONV-18).
    private sealed record OwnerInput : IOwnerInput
    {
        // A Persian name is the normal case, not an edge case: the length rule counts characters,
        // never bytes, and nothing here may restrict the alphabet.
        public string FullName { get; init; } = "مریم رضایی";

        public string MobileNumber { get; init; } = "09121234567";

        public string? LandlineNumber { get; init; }

        public string? NationalId { get; init; }

        public string? Address { get; init; }

        public string? City { get; init; }

        public string? Notes { get; init; }

        // A fixed UTC literal rather than a clock read: this test double stands in for a validated
        // command (CONV-18), and a deterministic default keeps every existing assertion below
        // exercising the fields it actually names instead of an incidentally-failing intake date.
        public DateTime IntakeDateUtc { get; init; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void A_minimal_owner_with_name_and_mobile_is_valid()
    {
        // Landline and national ID stay null: item 9's decision makes them optional, because an
        // owner is often registered at the counter with nothing else to hand.
        _validator.Validate(new OwnerInput()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void A_fully_populated_owner_is_valid()
    {
        var owner = new OwnerInput
        {
            LandlineNumber = "02112345678",
            NationalId = "0084575948",
        };

        _validator.Validate(owner).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Full_name_is_required(string fullName)
    {
        ValidationResult result = _validator.Validate(new OwnerInput { FullName = fullName });

        result.FailedProperties().Should().Contain(nameof(IOwnerInput.FullName));
    }

    [Fact]
    public void Full_name_is_capped_at_200_characters()
    {
        _validator.Validate(new OwnerInput { FullName = new string('م', 200) }).IsValid.Should().BeTrue();
        _validator.Validate(new OwnerInput { FullName = new string('م', 201) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("0912123456")]      // 10 digits
    [InlineData("091212345678")]    // 12 digits
    [InlineData("+989121234567")]   // E.164 form: normalized at the UI edge, not accepted here
    [InlineData("9121234567")]      // missing the leading 0
    [InlineData("08121234567")]     // landline prefix in the mobile field
    [InlineData("0912 123 4567")]   // separators
    [InlineData("۰۹۱۲۱۲۳۴۵۶۷")]     // Persian digits throughout
    [InlineData("09۱۲۱۲۳۴۵۶۷")]     // ASCII prefix, Persian tail
    public void Mobile_number_must_be_an_ascii_11_digit_09_number(string mobileNumber)
    {
        ValidationResult result = _validator.Validate(new OwnerInput { MobileNumber = mobileNumber });

        result.FailedProperties().Should().Contain(nameof(IOwnerInput.MobileNumber));
    }

    [Theory]
    [InlineData("09121234567")]
    [InlineData("09351234567")]
    [InlineData("09901234567")]
    public void Mobile_number_accepts_any_operator_prefix(string mobileNumber)
    {
        // Operator ranges are reassigned over time; pinning them would reject real numbers, so the
        // rule stops at "09" plus length.
        _validator.Validate(new OwnerInput { MobileNumber = mobileNumber }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Landline_number_is_optional(string? landlineNumber)
    {
        _validator.Validate(new OwnerInput { LandlineNumber = landlineNumber }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("02112345678")]  // Tehran, 11 digits
    [InlineData("0341234567")]   // 10-digit area/subscriber split
    public void Landline_number_accepts_10_and_11_digit_forms(string landlineNumber)
    {
        _validator.Validate(new OwnerInput { LandlineNumber = landlineNumber }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("2112345678")]     // no leading 0
    [InlineData("021123456")]      // too short
    [InlineData("021123456789")]   // too long
    [InlineData("021-1234-5678")]  // separators
    [InlineData("۰۲۱۱۲۳۴۵۶۷۸")]    // Persian digits
    public void Landline_number_is_rejected_when_it_is_not_an_ascii_area_code_number(string landlineNumber)
    {
        ValidationResult result = _validator.Validate(new OwnerInput { LandlineNumber = landlineNumber });

        result.FailedProperties().Should().Contain(nameof(IOwnerInput.LandlineNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void National_id_is_optional(string? nationalId)
    {
        _validator.Validate(new OwnerInput { NationalId = nationalId }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("0084575948")]
    [InlineData("0064567672")]
    [InlineData("1234567891")]
    [InlineData("0000000140")]  // checksum remainder 0 — the mod-11 branch that is not 11-remainder
    [InlineData("0000000061")]  // checksum remainder 1 — same branch, other value
    public void National_id_accepts_a_code_that_passes_the_mod_11_checksum(string nationalId)
    {
        _validator.Validate(new OwnerInput { NationalId = nationalId }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("0084575947")]   // last digit off by one
    [InlineData("0012345678")]   // plausible-looking, wrong check digit
    [InlineData("008457594")]    // 9 digits
    [InlineData("00845759480")]  // 11 digits
    [InlineData("008457594a")]   // non-digit
    [InlineData("۰۰۸۴۵۷۵۹۴۸")]   // Persian digits: char.IsAsciiDigit rejects them
    [InlineData("1111111111")]   // passes the arithmetic, never issued
    [InlineData("0000000000")]
    public void National_id_is_rejected_when_the_checksum_or_shape_is_wrong(string nationalId)
    {
        ValidationResult result = _validator.Validate(new OwnerInput { NationalId = nationalId });

        result.FailedProperties().Should().Contain(nameof(IOwnerInput.NationalId));
    }

    [Fact]
    public void Every_broken_field_is_reported_in_one_pass()
    {
        // A form binds the whole failure list at once; stopping at the first error would make the
        // user fix one field per round trip.
        var owner = new OwnerInput
        {
            FullName = "",
            MobileNumber = "123",
            LandlineNumber = "12",
            NationalId = "1",
        };

        ValidationResult result = _validator.Validate(owner);

        result.FailedProperties().Should().BeEquivalentTo(
            nameof(IOwnerInput.FullName),
            nameof(IOwnerInput.MobileNumber),
            nameof(IOwnerInput.LandlineNumber),
            nameof(IOwnerInput.NationalId));
    }

    [Fact]
    public void Validation_runs_with_no_io()
    {
        ValidatorContract.ShouldRunWithoutIo(_validator, new OwnerInput());
    }

    [Fact]
    public void Every_owner_input_property_is_covered_by_a_rule()
    {
        ValidatorContract.ShouldRuleOnEveryPropertyOf(_validator);
    }
}
