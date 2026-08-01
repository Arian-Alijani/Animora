using Animora.SharedKernel.Primitives;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.SharedKernel;

public class ResultTests
{
    private static readonly Error NotFound = new("ERR-CLIENTS-001", "Owner 42 does not exist.");

    [Fact]
    public void Success_reports_success_and_exposes_no_error()
    {
        Result result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_reports_failure_and_carries_the_error()
    {
        Result result = Result.Failure(NotFound);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(NotFound);
    }

    [Fact]
    public void Error_access_on_a_success_throws()
    {
        Result result = Result.Success();

        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Success_with_a_value_exposes_it()
    {
        Result<int> result = Result.Success(7);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Fact]
    public void Value_access_on_a_failure_throws_and_names_the_code()
    {
        Result<int> result = Result.Failure<int>(NotFound);

        var act = () => result.Value;

        // Handing back default(int) instead would turn a handled failure into a silent 0 several
        // layers downstream; the code in the message is what makes the caller bug diagnosable.
        act.Should().Throw<InvalidOperationException>().WithMessage($"*{NotFound.Code}*");
    }

    [Fact]
    public void A_failed_result_of_a_reference_type_does_not_leak_null_as_a_value()
    {
        Result<string> result = Result.Failure<string>(NotFound);

        result.IsFailure.Should().BeTrue();

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_rejects_a_missing_error()
    {
        // A failure with no reason is indistinguishable from a success at every call site that
        // branches on IsSuccess, so it is rejected at construction rather than at read time.
        var act = () => Result.Failure(null!);
        var actOfT = () => Result.Failure<int>(null!);

        act.Should().Throw<ArgumentNullException>();
        actOfT.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void A_valued_result_is_usable_wherever_a_valueless_one_is()
    {
        // Result<T> deriving from Result is what lets a handler pipeline inspect IsSuccess without
        // knowing the payload type; the two shapes stay exhaustive because nothing else can derive.
        Result result = Result.Success(7);

        result.IsSuccess.Should().BeTrue();
        result.Should().BeOfType<Result<int>>();
    }

    [Fact]
    public void Valueless_successes_share_one_instance()
    {
        // No state to carry, so the success path allocates nothing per call.
        Result.Success().Should().BeSameAs(Result.Success());
    }
}
