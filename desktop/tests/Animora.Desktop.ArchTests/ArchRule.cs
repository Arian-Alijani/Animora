using FluentAssertions;
using NetArchTest.Rules;

namespace Animora.Desktop.ArchTests;

internal static class ArchRule
{
    internal static void ShouldPass(this ConditionList conditions, string ruleId)
    {
        TestResult result = conditions.GetResult();

        // A rule whose type set is still empty passes; NetArchTest reports no failing types, which is
        // the intended behaviour for rules that guard phases not yet implemented.
        result.IsSuccessful.Should().BeTrue(
            "{0} is violated by: {1}",
            ruleId,
            string.Join(", ", result.FailingTypeNames ?? []));
    }
}
