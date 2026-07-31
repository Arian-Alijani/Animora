namespace Animora.Desktop.UI.AppState;

/// <summary>
/// The three states of DESK-ARCH-07's offline/online/syncing model. None of them gates input:
/// <see cref="Offline"/> means "writes stay local and queue in the outbox", not "wait"
/// (INV-15, DESK-ARCH-10).
/// </summary>
public enum ConnectivityStatus
{
    Online,
    Offline,
    Syncing,
}
