using Animora.Desktop.UI.AppState;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Animora.Desktop.App.AppState;

/// <summary>
/// Phase-02 stand-in for <see cref="IAppStatusState"/>: a fixed <see cref="ConnectivityStatus.Online"/>
/// with writes available. It runs no probe and owns no timer, because both inputs of the real state
/// belong to seams that stay empty in phase 1 — connectivity/sync to <c>Animora.Desktop.Sync</c>
/// (DT-12) and the degraded flag to the license heartbeat job.
/// <para>
/// Deriving from <see cref="ObservableObject"/> now means the swaps below only turn these constants
/// into <c>SetProperty</c>-backed properties; the status indicator bound to this instance stays as it
/// is.
/// </para>
/// </summary>
// TODO(P1-12): drive IsReadOnlyDegraded from the licensing state machine (LIC-12).
// TODO(P2): drive Connectivity from the connectivity probe and the sync cycle (DESK-ARCH-07).
public sealed class AppStatusState : ObservableObject, IAppStatusState
{
    /// <inheritdoc />
    public ConnectivityStatus Connectivity => ConnectivityStatus.Online;

    /// <inheritdoc />
    public bool IsReadOnlyDegraded => false;
}
