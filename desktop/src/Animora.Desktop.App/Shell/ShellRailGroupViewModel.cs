namespace Animora.Desktop.App.Shell;

/// <summary>
/// One <c>Overline</c>-headed block of rail pills (design-reference §6). Immutable: rail composition
/// is fixed once every module has registered its routes, and only each item's active flag changes
/// afterwards.
/// </summary>
public sealed class ShellRailGroupViewModel
{
    public ShellRailGroupViewModel(string title, IReadOnlyList<ShellRailItemViewModel> items)
    {
        Title = title;
        Items = items;
    }

    public string Title { get; }

    public IReadOnlyList<ShellRailItemViewModel> Items { get; }
}
