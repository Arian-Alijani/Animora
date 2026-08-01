using Animora.Desktop.Modules.Reporting.Data;
using Animora.Desktop.Modules.Reporting.Handlers;
using Animora.Desktop.Modules.Reporting.Models;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Animora.Desktop.UnitTests.Modules.Reporting;

/// <summary>
/// The Stage A/C data seam from the handler's side (DIR-03, DT-03): every test here substitutes
/// <see cref="IHomeSummaryReadStore"/>, which is only possible because the handler takes the interface
/// the module itself declared and nothing else — no <c>DbContext</c>, no connection, no clock. Phase
/// 21 swaps the in-memory fake for the Dapper-backed reader behind that same interface, so these tests
/// keep passing unchanged and are the proof that the swap needs no handler edit.
/// <para>
/// Every module UI phase copies this shape for its own handler; the read store is the only thing that
/// changes.
/// </para>
/// </summary>
public class GetHomeSummaryHandlerTests
{
    [Fact]
    public async Task Handle_returns_the_read_model_the_store_produced_untouched()
    {
        HomeSummary expected = SampleSummary();
        IHomeSummaryReadStore readStore = Substitute.For<IHomeSummaryReadStore>();
        readStore.GetSummaryAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(expected));
        GetHomeSummaryHandler handler = new(readStore);

        HomeSummary actual = await handler.Handle(new GetHomeSummaryQuery(), CancellationToken.None);

        // Same instance, not an equal copy: the read is a pass-through, so no projection, rounding or
        // unit conversion may creep in between the seam and the screen (DESK-ARCH-14 keeps those at
        // the binding edge).
        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Handle_forwards_the_cancellation_token_to_the_store()
    {
        IHomeSummaryReadStore readStore = Substitute.For<IHomeSummaryReadStore>();
        readStore.GetSummaryAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(SampleSummary()));
        GetHomeSummaryHandler handler = new(readStore);
        using CancellationTokenSource cancellation = new();

        _ = await handler.Handle(new GetHomeSummaryQuery(), cancellation.Token);

        // The Stage C implementation is a SQLite round trip, so the screen's token has to reach it —
        // otherwise a navigation away from the screen leaves the query running.
        await readStore.Received(1).GetSummaryAsync(cancellation.Token);
    }

    [Fact]
    public async Task Handle_lets_a_store_failure_surface_to_the_caller()
    {
        IHomeSummaryReadStore readStore = Substitute.For<IHomeSummaryReadStore>();
        readStore.GetSummaryAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HomeSummary>(new InvalidOperationException("read store unavailable")));
        GetHomeSummaryHandler handler = new(readStore);

        Func<Task> handle = async () => await handler.Handle(new GetHomeSummaryQuery(), CancellationToken.None);

        // No swallowing and no empty-summary fallback: a failed read is the ViewModel's error state to
        // show, not a silently blank screen.
        await handle.Should().ThrowAsync<InvalidOperationException>();
    }

    // Storage units and a UTC instant, exactly as the Stage C reader will return them (INV-05,
    // INV-13): a handler test that used display values would hide a unit conversion sneaking in here.
    private static HomeSummary SampleSummary() => new(
        TodayVisitCount: 18,
        TodayAppointmentCount: 24,
        OutstandingInvoiceCount: 6,
        TodayRevenueInRials: 184_500_000m,
        GeneratedAtUtc: new DateTime(2026, 7, 31, 6, 45, 0, DateTimeKind.Utc));
}
