using Animora.Contracts.Errors;
using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.SharedKernel.Primitives;
using Animora.SharedKernel.Validation.Clients;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Animora.Desktop.UnitTests.Modules.Clients;

/// <summary>
/// SaveOwnerHandler's one seam substituted (DT-02/DT-03): unlike
/// <c>SaveStaffMemberHandler</c>/<c>SavePatientHandler</c>, no lookup-dependent rule runs between
/// <see cref="OwnerValidator"/> and the write, because phase 05 TODO item 4's documented answer
/// means a duplicate <see cref="IOwnerInput.MobileNumber"/> is never rejected — this suite exercises
/// that validate-then-write shape without a DbContext or connection.
/// </summary>
public class SaveOwnerHandlerTests
{
    private readonly IOwnerWriteStore _writeStore = Substitute.For<IOwnerWriteStore>();
    private readonly SaveOwnerHandler _handler;

    public SaveOwnerHandlerTests()
    {
        _handler = new SaveOwnerHandler(_writeStore);
    }

    private static SaveOwnerCommand ValidCommand(Guid? ownerId = null, string mobileNumber = "09121234567") =>
        new(
            ownerId,
            "زهرا صادقی",
            mobileNumber,
            LandlineNumber: null,
            NationalId: null,
            Address: "خیابان ولیعصر",
            City: "تهران",
            Notes: "پرداخت نقدی",
            IntakeDateUtc: DateTime.UtcNow);

    [Fact]
    public async Task A_validation_failure_returns_the_code_and_writes_nothing()
    {
        // An 11-digit-starting-with-09 mobile is OwnerValidator's own format rule (SH-05); this one
        // is missing the "09" lead, so validation rejects it before the handler ever reaches the
        // write store.
        var command = ValidCommand(mobileNumber: "12345");

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ClientsErrors.ValidationFailed);
        await _writeStore.DidNotReceive().SaveAsync(Arg.Any<Guid>(), Arg.Any<IOwnerInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_create_assigns_a_fresh_uuid_v7_and_writes_through_the_store()
    {
        var command = ValidCommand();

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _writeStore.Received(1).SaveAsync(result.Value, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_edit_writes_through_the_owner_id_the_command_already_carries()
    {
        var ownerId = Guid.CreateVersion7();
        var command = ValidCommand(ownerId);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ownerId);
        await _writeStore.Received(1).SaveAsync(ownerId, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_mobile_number_already_held_by_another_owner_is_not_rejected()
    {
        // Phase 05 TODO item 4's documented answer: a mobile number identifies a contact channel,
        // not a sign-in identity (SEC-03), so two owners may share one — the handler enforces no
        // duplicate-mobile rule at all, unlike SaveStaffMemberHandler's UsernameAlreadyTaken check.
        var firstOwner = await _handler.Handle(ValidCommand(mobileNumber: "09121234567"), CancellationToken.None);
        var secondOwner = await _handler.Handle(ValidCommand(mobileNumber: "09121234567"), CancellationToken.None);

        firstOwner.IsSuccess.Should().BeTrue();
        secondOwner.IsSuccess.Should().BeTrue();
        secondOwner.Value.Should().NotBe(firstOwner.Value);
        await _writeStore.Received(2).SaveAsync(Arg.Any<Guid>(), Arg.Any<IOwnerInput>(), Arg.Any<CancellationToken>());
    }
}
