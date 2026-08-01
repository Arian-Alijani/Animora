using Animora.SharedKernel.Validation.Identity;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class CredentialValidatorTests
{
    private readonly CredentialValidator _validator = new();

    // The query shape phase 04's login screen sends in; the record stands in for it so the rules are
    // exercised through ICredentialInput exactly as SignInQuery will be (CONV-18).
    private sealed record CredentialInput : ICredentialInput
    {
        public string Username { get; init; } = "petshop";

        public string Password { get; init; } = "Petshop@123";
    }

    [Fact]
    public void A_username_and_password_pair_is_valid()
    {
        _validator.Validate(new CredentialInput()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Username_is_required(string username)
    {
        ValidationResult result = _validator.Validate(new CredentialInput { Username = username });

        result.FailedProperties().Should().Contain(nameof(ICredentialInput.Username));
    }

    [Fact]
    public void Username_is_capped_at_64_characters()
    {
        // No format rule here — StaffValidator already fixes the real shape (CredentialValidator's
        // own remark); this bound is only the anti-abuse buffer limit CredentialValidator.UsernameMaximumLength states.
        _validator.Validate(new CredentialInput { Username = new string('a', 64) }).IsValid.Should().BeTrue();
        _validator.Validate(new CredentialInput { Username = new string('a', 65) }).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Password_is_required(string password)
    {
        ValidationResult result = _validator.Validate(new CredentialInput { Password = password });

        result.FailedProperties().Should().Contain(nameof(ICredentialInput.Password));
    }

    [Fact]
    public void Password_is_capped_at_128_characters()
    {
        // No minimum length, complexity rule or character class: password policy is the server's,
        // applied where a password is set (CredentialValidator's own remark, SEC-01/SEC-03).
        _validator.Validate(new CredentialInput { Password = new string('a', 128) }).IsValid.Should().BeTrue();
        _validator.Validate(new CredentialInput { Password = new string('a', 129) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Every_broken_field_is_reported_in_one_pass()
    {
        var credential = new CredentialInput { Username = "", Password = "" };

        ValidationResult result = _validator.Validate(credential);

        result.FailedProperties().Should().BeEquivalentTo(
            nameof(ICredentialInput.Username),
            nameof(ICredentialInput.Password));
    }

    [Fact]
    public void Validation_runs_with_no_io()
    {
        ValidatorContract.ShouldRunWithoutIo(_validator, new CredentialInput());
    }

    [Fact]
    public void Every_credential_input_property_is_covered_by_a_rule()
    {
        ValidatorContract.ShouldRuleOnEveryPropertyOf(_validator);
    }
}
