namespace Animora.Desktop.UI.Services;

/// <summary>
/// Non-blocking toast surface for module ViewModels. Backs the persistent, never-modal status
/// surfaces DESK-ARCH-07 (connectivity/sync state) and DESK-ARCH-11 (long-operation progress: toast
/// + job center, never a blocking spinner) call for — a ViewModel raises a toast and moves on, it
/// never awaits a user dismissal.
/// </summary>
public interface IToastService
{
    /// <summary>Shows a neutral/informational toast.</summary>
    void ShowInfo(string message, string? title = null);

    /// <summary>Shows a success toast (e.g. a command completed).</summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>Shows a warning toast (e.g. a degraded/offline state, INV-15).</summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>Shows an error toast (e.g. a command failed validation or was rejected).</summary>
    void ShowError(string message, string? title = null);
}
