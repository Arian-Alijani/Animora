using Animora.Desktop.UI.Mvvm;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// Everything a module tells the shell about one screen (DESK-ARCH-05, DT-09). It lives in this leaf
/// project because a module may not reference the composition root (AT-09), yet must register its own
/// routes; the shell consumes descriptors it never names a type from.
/// <para>
/// The factory pair is the whole coupling seam: <see cref="CreateViewModel"/> pulls the ViewModel out
/// of the container the composition root built (so the ViewModel keeps its normal constructor
/// injection), and <see cref="CreateView"/> pairs it with its View. Use <see cref="Create{TViewModel,TView}"/>
/// instead of assembling the pair by hand.
/// </para>
/// </summary>
public sealed record RouteDescriptor
{
    /// <summary>Stable navigation key, kebab-case, e.g. <c>home</c> or <c>clients-list</c>.</summary>
    public required string RouteKey { get; init; }

    /// <summary>Persian display title, used for the rail label and the shell's page title.</summary>
    public required string Title { get; init; }

    /// <summary>Material Design glyph key, e.g. <c>mdi-home</c> (DESK-ARCH-13).</summary>
    public required string IconGlyph { get; init; }

    /// <summary>Rail group, or <see langword="null"/> for a route reachable only from another screen.</summary>
    public RailGroup? RailGroup { get; init; }

    /// <summary>Order within the rail group; equal values keep registration order.</summary>
    public int RailOrder { get; init; }

    /// <summary>Optional static count badge. Live counts arrive with their owning module's phase.</summary>
    public int? BadgeValue { get; init; }

    /// <summary>Resolves this route's ViewModel from the app container.</summary>
    public required Func<IServiceProvider, ViewModelBase> CreateViewModel { get; init; }

    /// <summary>Builds this route's View around an already-resolved ViewModel.</summary>
    public required Func<ViewModelBase, Control> CreateView { get; init; }

    /// <summary>
    /// Builds a descriptor for the common case: <typeparamref name="TViewModel"/> comes from DI and
    /// <typeparamref name="TView"/> is a parameterless View bound to it through <c>DataContext</c>
    /// (DESK-ARCH-01 — the View never reaches for the ViewModel itself).
    /// </summary>
    public static RouteDescriptor Create<TViewModel, TView>(
        string routeKey,
        string title,
        string iconGlyph,
        RailGroup? railGroup = null,
        int railOrder = 0,
        int? badgeValue = null)
        where TViewModel : ViewModelBase
        where TView : Control, new()
    {
        return new RouteDescriptor
        {
            RouteKey = routeKey,
            Title = title,
            IconGlyph = iconGlyph,
            RailGroup = railGroup,
            RailOrder = railOrder,
            BadgeValue = badgeValue,
            CreateViewModel = static provider => provider.GetRequiredService<TViewModel>(),
            CreateView = static viewModel => new TView { DataContext = viewModel },
        };
    }

    /// <summary>
    /// The rail-facing projection of this descriptor, or <see langword="null"/> when the route is not
    /// rail-visible.
    /// </summary>
    public NavigationEntry? ToRailEntry()
    {
        return RailGroup is null
            ? null
            : new NavigationEntry(RouteKey, Title, IconGlyph, RailGroup.Value, RailOrder, BadgeValue);
    }
}
