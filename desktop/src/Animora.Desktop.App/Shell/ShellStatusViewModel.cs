using System.ComponentModel;
using Animora.Desktop.UI.AppState;
using Animora.Desktop.UI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Animora.Desktop.App.Shell;

/// <summary>
/// Projects <see cref="IAppStatusState"/> onto the label/accent pairs <c>StatusIndicator</c> renders
/// (DESK-ARCH-07/08). The mapping lives here rather than in the view or a converter so the indicator
/// stays declarative and <see cref="ShellViewModel"/> stays about navigation.
/// <para>
/// It exposes state only — no command, nothing to await — which is what keeps the connectivity
/// surface non-blocking by construction (INV-15).
/// </para>
/// </summary>
public sealed class ShellStatusViewModel : ObservableObject
{
    private readonly IAppStatusState _appStatus;

    public ShellStatusViewModel(IAppStatusState appStatus)
    {
        _appStatus = appStatus;

        // Both this view model and the app-state service are container singletons that live for the
        // whole process, so this subscription has no detach point to pair with — and the indicator is
        // required to keep reflecting connectivity until shutdown (DESK-ARCH-07).
        _appStatus.PropertyChanged += OnAppStatusChanged;
    }

    public string ConnectivityText => _appStatus.Connectivity switch
    {
        ConnectivityStatus.Online => ShellText.StatusOnline,
        ConnectivityStatus.Offline => ShellText.StatusOffline,
        ConnectivityStatus.Syncing => ShellText.StatusSyncing,
        _ => string.Empty,
    };

    /// <summary>
    /// Accent for the connectivity chip. <see cref="ConnectivityStatus.Offline"/> is a warning, never a
    /// danger: offline is a normal working mode where writes queue locally (DESK-ARCH-07, INV-15), so
    /// only the read-only licensing state below earns the danger accent.
    /// </summary>
    public AccentVariant ConnectivityVariant => _appStatus.Connectivity switch
    {
        ConnectivityStatus.Offline => AccentVariant.Warning,
        ConnectivityStatus.Syncing => AccentVariant.Info,
        _ => AccentVariant.Success,
    };

    /// <summary>Shows the second chip: the one state that actually withdraws write availability
    /// (DESK-ARCH-08, LIC-12), which is why it is surfaced beside connectivity instead of replacing it.</summary>
    public bool IsReadOnlyDegraded => _appStatus.IsReadOnlyDegraded;

    private void OnAppStatusChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Two source properties feed three derived members; re-raising all three costs less than a
        // name-to-name map that would have to be kept in sync with IAppStatusState.
        OnPropertyChanged(nameof(ConnectivityText));
        OnPropertyChanged(nameof(ConnectivityVariant));
        OnPropertyChanged(nameof(IsReadOnlyDegraded));
    }
}
