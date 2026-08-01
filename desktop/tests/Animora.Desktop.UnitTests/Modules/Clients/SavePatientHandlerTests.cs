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
/// SavePatientHandler's two seams substituted (DT-02/DT-03): the one lookup-dependent rule
/// <see cref="PatientValidator"/> deliberately leaves to the handler — <see cref="IOwnerReadStore"/>'s
/// owner-existence check (DOM-03) — is exercised without a DbContext or connection, mirroring
/// <c>SaveStaffMemberHandlerTests</c>'s own role-lookup shape.
/// </summary>
public class SavePatientHandlerTests
{
    private static readonly Guid KnownOwnerId = Guid.CreateVersion7();

    private readonly IPatientWriteStore _patientWriteStore = Substitute.For<IPatientWriteStore>();
    private readonly IOwnerReadStore _ownerReadStore = Substitute.For<IOwnerReadStore>();
    private readonly SavePatientHandler _handler;

    public SavePatientHandlerTests()
    {
        _handler = new SavePatientHandler(_patientWriteStore, _ownerReadStore);

        // Known by default; the not-found test narrows this to simulate a stale/removed owner id.
        _ownerReadStore.ExistsAsync(KnownOwnerId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
    }

    private static SavePatientCommand ValidCommand(Guid? patientId = null, Guid? ownerId = null) =>
        new(
            patientId,
            ownerId ?? KnownOwnerId,
            "پیشی",
            "Cat",
            "Female",
            Breed: "پرشین",
            BirthDateUtc: DateTime.UtcNow.Date,
            WeightKg: 3.4m,
            MicrochipId: null,
            MicrochipImplantedAtUtc: null,
            Color: "سفید",
            Temperament: "آرام",
            HousingType: "Apartment",
            Diet: null,
            BarcodeValue: null,
            SurgicalHistory: null,
            IsBirthDateEstimated: false,
            IsSterilized: true);

    [Fact]
    public async Task A_validation_failure_returns_the_code_and_writes_nothing()
    {
        // "Feline" is not in PatientValidator.AllowedSpecies (SH-05) — the owner id is otherwise
        // valid, so a pass here would prove the lookup ran before validation, which it must not.
        var command = ValidCommand() with { Species = "Feline" };

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ClientsErrors.ValidationFailed);
        await _ownerReadStore.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _patientWriteStore.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<IPatientInput>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unknown_owner_returns_owner_not_found_and_writes_nothing()
    {
        var unknownOwnerId = Guid.CreateVersion7();
        _ownerReadStore.ExistsAsync(unknownOwnerId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

        var command = ValidCommand(ownerId: unknownOwnerId);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ClientsErrors.OwnerNotFound);
        await _patientWriteStore.DidNotReceive().SaveAsync(
            Arg.Any<Guid>(), Arg.Any<IPatientInput>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_create_assigns_a_fresh_uuid_v7_and_writes_through_the_store()
    {
        var command = ValidCommand();

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _patientWriteStore.Received(1).SaveAsync(
            result.Value, command, command.IsBirthDateEstimated, command.IsSterilized, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_edit_writes_through_the_patient_id_the_command_already_carries()
    {
        var patientId = Guid.CreateVersion7();
        var command = ValidCommand(patientId);

        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(patientId);
        await _patientWriteStore.Received(1).SaveAsync(
            patientId, command, command.IsBirthDateEstimated, command.IsSterilized, Arg.Any<CancellationToken>());
    }
}
