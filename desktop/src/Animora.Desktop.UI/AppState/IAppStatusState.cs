using System.ComponentModel;

namespace Animora.Desktop.UI.AppState;

/// <summary>
/// Connectivity/sync and licensing state for the shell's persistent status indicator
/// (DESK-ARCH-07/08), as one injected singleton rather than per-screen state (DESK-ARCH-03).
/// <para>
/// The contract is non-blocking by construction: it exposes state to observe and no operation to
/// await, so no screen can turn a connectivity change into a modal wait (INV-15). Connectivity and
/// licensing are separate members on purpose — they come from independent sources (probe/sync cycle vs
/// license heartbeat) and can be true at the same time, and only
/// <see cref="IsReadOnlyDegraded"/> actually withdraws write availability (LIC-12).
/// </para>
/// </summary>
public interface IAppStatusState : INotifyPropertyChanged
{
    /// <summary>Current position in DESK-ARCH-07's state machine.</summary>
    ConnectivityStatus Connectivity { get; }

    /// <summary>
    /// <see langword="true"/> once the license grace period has lapsed: reads and exports stay full,
    /// new writes are refused (LIC-12), and the indicator surfaces it (DESK-ARCH-08).
    /// </summary>
    bool IsReadOnlyDegraded { get; }
}
