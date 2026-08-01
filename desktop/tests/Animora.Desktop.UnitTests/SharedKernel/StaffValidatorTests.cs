using Animora.SharedKernel.Validation.Identity;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class StaffValidatorTests
{
    private readonly StaffValidator _validator = new();

    // The command shape phase 04's staff form sends in; the record stands in for it so the rules are
    // exercised through IStaffInput exactly as SaveStaffMemberCommand will be (CONV-18).
    private sealed record StaffInput : IStaffInput
    {
        public string FullName { get; init; } = "زهرا صادقی";

        public string Username { get; init; } = "petshop-zsadeghi";

        public string MobileNumber { get; init; } = "09121234567";

        public string? Email { get; init; }

        public Guid RoleId { get; init; } = Guid.CreateVersion7();
    }

    [Fact]
    public void A_minimal_staff_member_with_no_email_is_valid()
    {
        _validator.Validate(new StaffInput()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void A_fully_populated_staff_member_is_valid()
    {
        var staff = new StaffInput { Email = "zahra.sadeghi@petshop.example" };

        _validator.Validate(staff).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Full_name_is_required(string fullName)
    {
        ValidationResult result = _validator.Validate(new StaffInput { FullName = fullName });

        result.FailedProperties().Should().Contain(nameof(IStaffInput.FullName));
    }

    [Fact]
    public void Full_name_is_capped_at_200_characters()
    {
        _validator.Validate(new StaffInput { FullName = new string('ز', 200) }).IsValid.Should().BeTrue();
        _validator.Validate(new StaffInput { FullName = new string('ز', 201) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]              // 2 characters: below UsernameMinimumLength
    [InlineData("Petshop")]         // upper-case: the pattern is lower-case ASCII only
    [InlineData("1petshop")]        // must start with a letter, not a digit
    [InlineData("petshop drahmadi")] // separators must be '.', '_' or '-', never a space
    [InlineData("پتشاپ")]           // Persian characters throughout
    [InlineData("petshop-درا")]     // ASCII prefix, Persian tail
    public void Username_is_rejected_when_it_is_not_a_lower_case_ascii_identifier(string username)
    {
        ValidationResult result = _validator.Validate(new StaffInput { Username = username });

        result.FailedProperties().Should().Contain(nameof(IStaffInput.Username));
    }

    [Theory]
    [InlineData("abc")]                  // exactly the 3-character minimum
    [InlineData("petshop-drahmadi")]
    [InlineData("petshop.drahmadi")]
    [InlineData("petshop_drahmadi")]
    [InlineData("petshop123")]
    public void Username_accepts_lower_case_ascii_letters_digits_dot_underscore_and_hyphen(string username)
    {
        _validator.Validate(new StaffInput { Username = username }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Username_is_capped_at_64_characters()
    {
        _validator.Validate(new StaffInput { Username = "a" + new string('b', 63) }).IsValid.Should().BeTrue();
        _validator.Validate(new StaffInput { Username = "a" + new string('b', 64) }).IsValid.Should().BeFalse();
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
    public void Mobile_number_must_be_an_ascii_11_digit_09_number(string mobileNumber)
    {
        ValidationResult result = _validator.Validate(new StaffInput { MobileNumber = mobileNumber });

        result.FailedProperties().Should().Contain(nameof(IStaffInput.MobileNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Email_is_optional(string? email)
    {
        _validator.Validate(new StaffInput { Email = email }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.example")]
    [InlineData("@no-local-part.example")]
    [InlineData("trailing-at@")]
    public void Email_is_rejected_when_it_is_not_a_plausible_address(string email)
    {
        ValidationResult result = _validator.Validate(new StaffInput { Email = email });

        result.FailedProperties().Should().Contain(nameof(IStaffInput.Email));
    }

    [Fact]
    public void Email_is_capped_at_254_characters()
    {
        // "a@" + filler + ".co": a shape EmailAddress() accepts at any filler length, so the
        // boundary itself is what each assertion below is isolating.
        static string BuildEmailOfLength(int totalLength) => "a@" + new string('b', totalLength - 5) + ".co";

        BuildEmailOfLength(254).Length.Should().Be(254);
        _validator.Validate(new StaffInput { Email = BuildEmailOfLength(254) }).IsValid.Should().BeTrue();
        _validator.Validate(new StaffInput { Email = BuildEmailOfLength(255) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Role_id_is_required()
    {
        // Existence of the referenced role is a lookup SaveStaffMemberHandler owns
        // (IdentityErrors.RoleNotFound); only the shape — "some role was selected" — is this rule's.
        ValidationResult result = _validator.Validate(new StaffInput { RoleId = Guid.Empty });

        result.FailedProperties().Should().Contain(nameof(IStaffInput.RoleId));
    }

    [Fact]
    public void Every_broken_field_is_reported_in_one_pass()
    {
        var staff = new StaffInput
        {
            FullName = "",
            Username = "1",
            MobileNumber = "123",
            Email = "not-an-email",
            RoleId = Guid.Empty,
        };

        ValidationResult result = _validator.Validate(staff);

        result.FailedProperties().Should().BeEquivalentTo(
            nameof(IStaffInput.FullName),
            nameof(IStaffInput.Username),
            nameof(IStaffInput.MobileNumber),
            nameof(IStaffInput.Email),
            nameof(IStaffInput.RoleId));
    }

    [Fact]
    public void Validation_runs_with_no_io()
    {
        ValidatorContract.ShouldRunWithoutIo(_validator, new StaffInput());
    }

    [Fact]
    public void Every_staff_input_property_is_covered_by_a_rule()
    {
        ValidatorContract.ShouldRuleOnEveryPropertyOf(_validator);
    }
}
