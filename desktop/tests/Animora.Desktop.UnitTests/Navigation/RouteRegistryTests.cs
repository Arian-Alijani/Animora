using Animora.Desktop.App.Navigation;
using Animora.Desktop.UI.Navigation;
using FluentAssertions;
using Xunit;

namespace Animora.Desktop.UnitTests.Navigation;

/// <summary>
/// DESK-ARCH-05's registration guarantees, which every module phase relies on without re-testing:
/// one module can never shadow another's route key, an unknown key fails loudly instead of silently
/// doing nothing, and the rail's order is a function of the descriptors — not of the order the
/// composition root happened to call the module extensions in.
/// </summary>
public class RouteRegistryTests
{
    [Fact]
    public void Register_rejects_a_second_route_with_the_same_key()
    {
        RouteRegistry registry = new();
        registry.Register(Route("home", RailGroup.CommandCenter));

        Action registerAgain = () => registry.Register(Route("home", RailGroup.ClinicManagement));

        // Named in the message so the failing module is identifiable from the launch crash alone.
        registerAgain.Should().Throw<InvalidOperationException>().WithMessage("*home*");
    }

    [Fact]
    public void Register_treats_route_keys_as_case_sensitive()
    {
        RouteRegistry registry = new();
        registry.Register(Route("home", RailGroup.CommandCenter));

        // Ordinal comparison (CONV-12): route keys are kebab-case identifiers, so a casing difference
        // is a typo the registry must surface as a *different* route rather than quietly match.
        Action registerOtherCasing = () => registry.Register(Route("Home", RailGroup.CommandCenter));

        registerOtherCasing.Should().NotThrow();
        registry.GetRequired("Home").Title.Should().Be("Home");
    }

    [Fact]
    public void GetRequired_fails_for_an_unregistered_key_and_names_the_keys_it_knows()
    {
        RouteRegistry registry = new();
        registry.Register(Route("home", RailGroup.CommandCenter));

        Action unknownKey = () => registry.GetRequired("clients-list");

        unknownKey.Should().Throw<KeyNotFoundException>()
            .WithMessage("*clients-list*")
            .WithMessage("*home*");
    }

    [Fact]
    public void RailEntries_are_ordered_by_group_then_rail_order_regardless_of_registration_order()
    {
        RouteRegistry registry = new();
        registry.Register(Route("finance-invoices", RailGroup.FinancialOperations, railOrder: 0));
        registry.Register(Route("clients-list", RailGroup.ClinicManagement, railOrder: 10));
        registry.Register(Route("home", RailGroup.CommandCenter, railOrder: 0));
        registry.Register(Route("clients-pets", RailGroup.ClinicManagement, railOrder: 5));

        registry.RailEntries.Select(entry => entry.RouteKey).Should()
            .Equal("home", "clients-pets", "clients-list", "finance-invoices");
    }

    [Fact]
    public void RailEntries_fall_back_to_registration_order_for_an_equal_rail_order()
    {
        RouteRegistry registry = new();
        registry.Register(Route("visits-today", RailGroup.ClinicManagement, railOrder: 0));
        registry.Register(Route("visits-history", RailGroup.ClinicManagement, railOrder: 0));

        registry.RailEntries.Select(entry => entry.RouteKey).Should()
            .Equal("visits-today", "visits-history");
    }

    [Fact]
    public void RailEntries_reflect_a_route_registered_after_a_previous_read()
    {
        RouteRegistry registry = new();
        registry.Register(Route("home", RailGroup.CommandCenter));
        _ = registry.RailEntries;

        registry.Register(Route("clients-list", RailGroup.ClinicManagement));

        // Guards the ordered-entries cache: composition registers module by module, and the shell may
        // read the rail at any point after that.
        registry.RailEntries.Select(entry => entry.RouteKey).Should().Equal("home", "clients-list");
    }

    [Fact]
    public void RailEntries_exclude_a_route_that_declares_no_rail_group()
    {
        RouteRegistry registry = new();
        registry.Register(Route("client-detail"));

        // A detail screen reached from a list is navigable but not rail-visible.
        registry.RailEntries.Should().BeEmpty();
        registry.GetRequired("client-detail").RouteKey.Should().Be("client-detail");
    }

    [Fact]
    public void RailEntries_carry_the_badge_value_as_the_descriptor_declared_it()
    {
        RouteRegistry registry = new();
        registry.Register(RouteDescriptor.Create<StubViewModel, StubView>(
            routeKey: "notifications",
            title: "اعلان‌ها",
            iconGlyph: "mdi-bell",
            railGroup: RailGroup.CommandCenter,
            railOrder: 1,
            badgeValue: 3));

        // Persian-digit formatting of the badge belongs to the shell's rail item view model, so the
        // registry must hand the raw count through untouched.
        registry.RailEntries.Should().ContainSingle().Which.BadgeValue.Should().Be(3);
    }

    // Title mirrors the route key so an ordering assertion can read as route keys only.
    private static RouteDescriptor Route(string routeKey, RailGroup? railGroup = null, int railOrder = 0) =>
        RouteDescriptor.Create<StubViewModel, StubView>(
            routeKey: routeKey,
            title: routeKey,
            iconGlyph: "mdi-circle",
            railGroup: railGroup,
            railOrder: railOrder);
}
