using System.Windows.Input;
using Animora.Desktop.UI.Localization;
using Animora.Desktop.UI.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Animora.Desktop.App.Shell;

/// <summary>
/// One rail pill as the shell binds it. It is a projection of a <see cref="NavigationEntry"/>, not a
/// second copy of the route: no factory reaches this type, so the rail can render a module's screen
/// entry without being able to construct it (DESK-ARCH-05).
/// <para>
/// <see cref="NavigateCommand"/> is the shell's single command instance shared by every item — the
/// route key travels as the command parameter instead of one closure per pill.
/// </para>
/// </summary>
public sealed class ShellRailItemViewModel : ObservableObject
{
    private bool _isActive;

    public ShellRailItemViewModel(NavigationEntry entry, string groupTitle, ICommand navigateCommand)
    {
        RouteKey = entry.RouteKey;
        Title = entry.Title;
        IconGlyph = entry.IconGlyph;
        GroupTitle = groupTitle;
        // Counts are Persian-Indic in every position, converted at the binding edge only
        // (DESK-ARCH-14, design-reference §2).
        BadgeText = entry.BadgeValue is { } badgeValue ? PersianNumberFormatter.FormatNumber(badgeValue) : null;
        NavigateCommand = navigateCommand;
    }

    public string RouteKey { get; }

    public string Title { get; }

    public string IconGlyph { get; }

    /// <summary>Heading of the rail group this item sits under; the shell's breadcrumb uses it as the
    /// coarse crumb, so the mapping is not duplicated per screen.</summary>
    public string GroupTitle { get; }

    public string? BadgeText { get; }

    public bool HasBadge => BadgeText is not null;

    public ICommand NavigateCommand { get; }

    /// <summary>Drives the active-pill style class; only <see cref="ShellViewModel"/> sets it, from the
    /// navigation service's route-changed notification rather than from the click that caused it.</summary>
    public bool IsActive
    {
        get => _isActive;
        internal set => SetProperty(ref _isActive, value);
    }
}
