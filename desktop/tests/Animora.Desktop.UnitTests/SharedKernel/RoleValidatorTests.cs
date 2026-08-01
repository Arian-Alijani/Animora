using Animora.SharedKernel.Validation.Identity;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class RoleValidatorTests
{
    private readonly RoleValidator _validator = new();

    // The command shape phase 04's role-management screen sends in; the record stands in for it so
    // the rules are exercised through IRoleInput exactly as SaveRoleCommand will be (CONV-18).
    private sealed record RoleInput : IRoleInput
    {
        public string DisplayName { get; init; } = "پذیرش";

        public IReadOnlyCollection<string> PermissionClaimKeys { get; init; } = ["owners.read"];
    }

    [Fact]
    public void A_role_with_a_display_name_and_one_claim_is_valid()
    {
        _validator.Validate(new RoleInput()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Display_name_is_required(string displayName)
    {
        ValidationResult result = _validator.Validate(new RoleInput { DisplayName = displayName });

        result.FailedProperties().Should().Contain(nameof(IRoleInput.DisplayName));
    }

    [Fact]
    public void Display_name_is_capped_at_100_characters()
    {
        _validator.Validate(new RoleInput { DisplayName = new string('پ', 100) }).IsValid.Should().BeTrue();
        _validator.Validate(new RoleInput { DisplayName = new string('پ', 101) }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Permission_claim_keys_must_not_be_empty()
    {
        // A role that grants nothing is not a role the screen should be able to save (SEC-09).
        ValidationResult result = _validator.Validate(new RoleInput { PermissionClaimKeys = [] });

        result.FailedProperties().Should().Contain(nameof(IRoleInput.PermissionClaimKeys));
    }

    [Fact]
    public void A_claim_key_repeated_in_the_same_role_is_rejected()
    {
        ValidationResult result = _validator.Validate(
            new RoleInput { PermissionClaimKeys = ["owners.read", "owners.read"] });

        result.FailedProperties().Should().Contain(nameof(IRoleInput.PermissionClaimKeys));
    }

    [Fact]
    public void Distinct_claim_keys_of_any_count_are_accepted()
    {
        _validator.Validate(new RoleInput
        {
            PermissionClaimKeys = ["owners.read", "owners.write", "patients.read"],
        }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("owners.read")]
    [InlineData("cash-session.open")]     // hyphenated segment
    [InlineData("reports.view-advanced")] // hyphenated action
    [InlineData("a.b.c")]                 // more than two segments: the pattern does not fix the count at two
    public void A_claim_key_in_resource_dot_action_form_is_accepted(string claimKey)
    {
        _validator.Validate(new RoleInput { PermissionClaimKeys = [claimKey] }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("owners")]        // no dot: not a {resource}.{action} pair
    [InlineData("Owners.Read")]   // upper-case: claim keys are lower-case identifiers (AG-12)
    [InlineData("owners.Read")]   // upper-case action segment alone
    [InlineData("1owners.read")]  // a segment may not start with a digit
    [InlineData("owners.")]       // trailing dot with no action segment
    [InlineData(".read")]         // leading dot with no resource segment
    [InlineData("-owners.read")]  // a segment may not start with a hyphen
    public void A_claim_key_outside_the_resource_dot_action_shape_is_rejected(string claimKey)
    {
        ValidationResult result = _validator.Validate(new RoleInput { PermissionClaimKeys = [claimKey] });

        // RuleForEach's default indexer names the failure "PermissionClaimKeys[0]" rather than the
        // bare property name the NotEmpty/duplicate rules above use.
        result.FailedProperties().Should()
            .Contain(name => name.StartsWith(nameof(IRoleInput.PermissionClaimKeys), StringComparison.Ordinal));
    }

    [Fact]
    public void Every_broken_field_is_reported_in_one_pass()
    {
        var role = new RoleInput { DisplayName = "", PermissionClaimKeys = [] };

        ValidationResult result = _validator.Validate(role);

        result.FailedProperties().Should().BeEquivalentTo(
            nameof(IRoleInput.DisplayName),
            nameof(IRoleInput.PermissionClaimKeys));
    }

    [Fact]
    public void Validation_runs_with_no_io()
    {
        ValidatorContract.ShouldRunWithoutIo(_validator, new RoleInput());
    }

    [Fact]
    public void Every_role_input_property_is_covered_by_a_rule()
    {
        ValidatorContract.ShouldRuleOnEveryPropertyOf(_validator);
    }
}
