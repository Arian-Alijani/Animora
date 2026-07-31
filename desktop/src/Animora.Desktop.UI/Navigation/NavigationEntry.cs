namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// One rail item as the shell renders it: display data only, no factories, so the rail can never
/// construct a screen by accident and binding a rail item never keeps a view/ViewModel alive.
/// Produced from a <see cref="RouteDescriptor"/> by <see cref="RouteDescriptor.ToRailEntry"/>.
/// </summary>
/// <param name="RouteKey">Key passed back to <c>INavigationService</c> when the item is activated.</param>
/// <param name="Title">Rail label; also the shell's page title for this route.</param>
/// <param name="IconGlyph">Material Design glyph key, e.g. <c>mdi-home</c> (DESK-ARCH-13).</param>
/// <param name="RailGroup">Group this item is listed under.</param>
/// <param name="RailOrder">Order within the group; ties fall back to registration order.</param>
/// <param name="BadgeValue">Optional count badge (design-reference.md §6 rail anatomy).</param>
public sealed record NavigationEntry(
    string RouteKey,
    string Title,
    string IconGlyph,
    RailGroup RailGroup,
    int RailOrder,
    int? BadgeValue);
