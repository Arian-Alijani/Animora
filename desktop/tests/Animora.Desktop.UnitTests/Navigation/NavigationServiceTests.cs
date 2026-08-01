using Animora.Desktop.App.Navigation;
using Animora.Desktop.UI.Navigation;
using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Animora.Desktop.UnitTests.Navigation;

/// <summary>
/// The other half of DESK-ARCH-05: a route key is all a caller needs, and everything else — resolving
/// the ViewModel out of the container the composition root built, handing over the navigation
/// parameter, building the View, telling the shell — happens behind this one call. A real
/// <see cref="ServiceCollection"/> backs the tests rather than a stubbed
/// <see cref="IServiceProvider"/>, because "the view model comes from DI" is the property under test.
/// </summary>
public class NavigationServiceTests
{
    private const string RouteKey = "home";
    private const string RouteTitle = "خانه";

    [Fact]
    public void NavigateTo_resolves_the_route_view_model_from_the_container()
    {
        StubViewModel viewModel = new();
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(viewModel)
            .BuildServiceProvider();
        NavigationService navigation = new(RegistryWithHomeRoute(), provider);
        RouteChangedEventArgs? raised = null;
        navigation.RouteChanged += (_, e) => raised = e;

        navigation.NavigateTo(RouteKey);

        // Same instance the container holds: the descriptor's factory went through DI instead of
        // constructing the view model itself, which is what lets a screen keep constructor injection.
        raised.Should().NotBeNull();
        raised!.Content.Should().BeOfType<StubView>().Which.DataContext.Should().BeSameAs(viewModel);
    }

    [Fact]
    public void NavigateTo_raises_route_changed_with_the_route_key_title_and_built_view()
    {
        using ServiceProvider provider = ProviderWithStubViewModel();
        NavigationService navigation = new(RegistryWithHomeRoute(), provider);
        RouteChangedEventArgs? raised = null;
        navigation.RouteChanged += (_, e) => raised = e;

        navigation.NavigateTo(RouteKey);

        raised.Should().NotBeNull();
        raised!.RouteKey.Should().Be(RouteKey);
        // The shell's page title/breadcrumb come from the descriptor, so no screen re-declares its own.
        raised.Title.Should().Be(RouteTitle);
        raised.Content.Should().BeOfType<StubView>();
        navigation.CurrentRouteKey.Should().Be(RouteKey);
    }

    [Fact]
    public void NavigateTo_hands_the_parameter_to_a_navigation_aware_view_model_before_the_shell_is_notified()
    {
        StubViewModel viewModel = new();
        using ServiceProvider provider = new ServiceCollection()
            .AddSingleton(viewModel)
            .BuildServiceProvider();
        NavigationService navigation = new(RegistryWithHomeRoute(), provider);
        object? parameterWhenRaised = null;
        navigation.RouteChanged += (_, _) => parameterWhenRaised = viewModel.NavigatedParameter;

        navigation.NavigateTo(RouteKey, parameter: 42);

        viewModel.NavigatedToCount.Should().Be(1);
        viewModel.NavigatedParameter.Should().Be(42);
        // Ordering matters: property changes raised in OnNavigatedTo must already be applied when the
        // shell binds the view.
        parameterWhenRaised.Should().Be(42);
    }

    [Fact]
    public void NavigateTo_navigates_a_view_model_that_is_not_navigation_aware()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddTransient<PlainViewModel>()
            .BuildServiceProvider();
        RouteRegistry registry = new();
        registry.Register(RouteDescriptor.Create<PlainViewModel, StubView>(
            routeKey: RouteKey,
            title: RouteTitle,
            iconGlyph: "mdi-home",
            railGroup: RailGroup.CommandCenter));
        NavigationService navigation = new(registry, provider);

        // INavigationAware is opt-in, so a parameter passed to a screen that ignores it is not an error.
        Action navigate = () => navigation.NavigateTo(RouteKey, parameter: "ignored");

        navigate.Should().NotThrow();
        navigation.CurrentRouteKey.Should().Be(RouteKey);
    }

    [Fact]
    public void NavigateTo_builds_a_fresh_view_model_and_view_on_every_navigation()
    {
        using ServiceProvider provider = ProviderWithStubViewModel();
        NavigationService navigation = new(RegistryWithHomeRoute(), provider);
        List<Control> contents = [];
        navigation.RouteChanged += (_, e) => contents.Add(e.Content);

        navigation.NavigateTo(RouteKey);
        navigation.NavigateTo(RouteKey);

        // Documented behaviour, not an accident: re-entering a screen re-runs its load request, which
        // is how a list shows rows written since it was left.
        contents.Should().HaveCount(2);
        contents[1].Should().NotBeSameAs(contents[0]);
        contents[1].DataContext.Should().NotBeSameAs(contents[0].DataContext);
    }

    [Fact]
    public void NavigateTo_fails_for_an_unregistered_route_key_and_keeps_the_current_screen()
    {
        using ServiceProvider provider = ProviderWithStubViewModel();
        NavigationService navigation = new(RegistryWithHomeRoute(), provider);
        int raisedCount = 0;
        navigation.RouteChanged += (_, _) => raisedCount++;
        navigation.NavigateTo(RouteKey);

        Action unknownKey = () => navigation.NavigateTo("clients-list");

        unknownKey.Should().Throw<KeyNotFoundException>();
        raisedCount.Should().Be(1);
        navigation.CurrentRouteKey.Should().Be(RouteKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NavigateTo_rejects_a_blank_route_key_before_touching_the_registry(string? routeKey)
    {
        using ServiceProvider provider = ProviderWithStubViewModel();
        NavigationService navigation = new(new RouteRegistry(), provider);

        Action blankKey = () => navigation.NavigateTo(routeKey!);

        // ArgumentNullException for null, ArgumentException for the blank strings — both are caller
        // bugs that must not degrade into "navigation silently did nothing".
        blankKey.Should().Throw<ArgumentException>();
    }

    private static RouteRegistry RegistryWithHomeRoute()
    {
        RouteRegistry registry = new();
        registry.Register(RouteDescriptor.Create<StubViewModel, StubView>(
            routeKey: RouteKey,
            title: RouteTitle,
            iconGlyph: "mdi-home",
            railGroup: RailGroup.CommandCenter));

        return registry;
    }

    private static ServiceProvider ProviderWithStubViewModel() =>
        new ServiceCollection()
            .AddTransient<StubViewModel>()
            .BuildServiceProvider();
}
