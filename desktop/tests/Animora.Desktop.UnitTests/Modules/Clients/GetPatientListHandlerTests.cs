using Animora.Desktop.Modules.Clients.Data;
using Animora.Desktop.Modules.Clients.Handlers;
using Animora.Desktop.Modules.Clients.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Animora.Desktop.UnitTests.Modules.Clients;

/// <summary>
/// <see cref="GetPatientListHandler"/> over a substituted <see cref="IPatientReadStore"/> (DT-02):
/// proves the one handler serves both the global list (no owner filter) and the owner-scoped list
/// (an owner filter) by forwarding <see cref="GetPatientListQuery.OwnerId"/> straight through — the
/// phase 05 TODO header's "one patient-list route serves both modes" decision (AG-14,
/// DESK-ARCH-05) — and that a keyset page's <see cref="PatientPage.NextCursor"/> round-trips back
/// in as the following query's <see cref="GetPatientListQuery.AfterId"/> (CONV-16), the same
/// substituted-seam shape <c>SavePatientHandlerTests</c> already establishes for this module.
/// </summary>
public class GetPatientListHandlerTests
{
    private readonly IPatientReadStore _readStore = Substitute.For<IPatientReadStore>();
    private readonly GetPatientListHandler _handler;

    public GetPatientListHandlerTests()
    {
        _handler = new GetPatientListHandler(_readStore);
    }

    private static Patient MakePatient(Guid? ownerId = null) =>
        new(
            Guid.CreateVersion7(),
            ownerId ?? Guid.CreateVersion7(),
            "صاحب حیوان آزمایشی",
            "بیمار آزمایشی",
            "Cat",
            "Female",
            Breed: null,
            BirthDateUtc: null,
            IsBirthDateEstimated: false,
            WeightKg: null,
            IsSterilized: false,
            MicrochipId: null,
            MicrochipImplantedAtUtc: null,
            Color: null,
            Temperament: null,
            HousingType: null,
            Diet: null,
            BarcodeValue: null,
            SurgicalHistory: null);

    [Fact]
    public async Task Global_mode_forwards_a_null_owner_id_to_the_read_store_and_returns_its_page()
    {
        var expectedPage = new PatientPage([MakePatient()], NextCursor: null);
        _readStore
            .GetPageAsync(null, "لوسی", null, 25, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPage));

        var query = new GetPatientListQuery(OwnerId: null, SearchTerm: "لوسی", AfterId: null, Limit: 25);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(expectedPage);
        await _readStore.Received(1).GetPageAsync(null, "لوسی", null, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Owner_scoped_mode_forwards_the_owner_id_to_the_read_store_and_returns_its_page()
    {
        var ownerId = Guid.CreateVersion7();
        var expectedPage = new PatientPage([MakePatient(ownerId)], NextCursor: null);
        _readStore
            .GetPageAsync(ownerId, null, null, 25, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPage));

        var query = new GetPatientListQuery(OwnerId: ownerId, SearchTerm: null, AfterId: null, Limit: 25);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(expectedPage);
        await _readStore.Received(1).GetPageAsync(ownerId, null, null, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_pages_next_cursor_round_trips_as_the_following_querys_after_id()
    {
        var firstPage = new PatientPage([MakePatient()], NextCursor: "cursor-1");
        _readStore
            .GetPageAsync(null, null, null, 1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));

        var firstQuery = new GetPatientListQuery(OwnerId: null, SearchTerm: null, AfterId: null, Limit: 1);
        var firstResult = await _handler.Handle(firstQuery, CancellationToken.None);

        firstResult.NextCursor.Should().Be("cursor-1");

        var secondPage = new PatientPage([MakePatient()], NextCursor: null);
        _readStore
            .GetPageAsync(null, null, firstResult.NextCursor, 1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(secondPage));

        var secondQuery = new GetPatientListQuery(OwnerId: null, SearchTerm: null, AfterId: firstResult.NextCursor, Limit: 1);
        var secondResult = await _handler.Handle(secondQuery, CancellationToken.None);

        secondResult.NextCursor.Should().BeNull();
        await _readStore.Received(1).GetPageAsync(null, null, "cursor-1", 1, Arg.Any<CancellationToken>());
    }
}
