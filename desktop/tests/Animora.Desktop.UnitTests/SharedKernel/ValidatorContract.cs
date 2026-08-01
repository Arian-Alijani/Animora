using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

namespace Animora.Desktop.UnitTests.SharedKernel;

// The two assertions every shared validator owes SH-05/CONV-18, written once so each validator's
// test class states them rather than re-deriving them (INV-02).
internal static class ValidatorContract
{
    // SH-05's executable form. A DB or HTTP lookup inside a rule can only be expressed as an async
    // component (MustAsync/CustomAsync/WhenAsync), and FluentValidation throws
    // AsyncValidatorInvokedSynchronouslyException out of the synchronous Validate the moment one is
    // registered. A green synchronous run therefore proves no I/O-shaped rule exists — and it is
    // also the call desktop handlers make while offline, so this is the real execution path, not a
    // proxy for it.
    internal static void ShouldRunWithoutIo<T>(IValidator<T> validator, T input)
    {
        var validate = () => validator.Validate(input);

        validate.Should().NotThrow(
            "SH-05: a shared validator must be runnable with no I/O, so it may hold no async rule component");

        // Repeating the call must produce the same verdict: a rule reading anything ambient (a
        // clock, a file, a connection) is the only way these two could disagree.
        validator.Validate(input).IsValid.Should().Be(validator.Validate(input).IsValid);
    }

    // A property that reaches persistence with no rule at all is the failure mode this catches:
    // adding a member to an input interface now fails here until its rule lands (CONV-18).
    internal static void ShouldRuleOnEveryPropertyOf<T>(IValidator<T> validator)
    {
        IEnumerable<string> validated = validator.CreateDescriptor()
            .GetMembersWithValidators()
            .Select(member => member.Key);

        IEnumerable<string> declared = typeof(T).GetProperties().Select(property => property.Name);

        declared.Should().BeSubsetOf(validated, "CONV-18: every input property carries at least one rule");
    }

    internal static IEnumerable<string> FailedProperties(this ValidationResult result) =>
        result.Errors.Select(failure => failure.PropertyName);
}
