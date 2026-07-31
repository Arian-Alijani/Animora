using Avalonia.Controls.Notifications;

namespace Animora.Desktop.UI.Services;

/// <summary>
/// <see cref="IToastService"/> over Avalonia's <see cref="INotificationManager"/>. The concrete
/// <see cref="WindowNotificationManager"/> instance is host-bound (it wraps a
/// <c>TopLevel</c>/window), so it is constructed and registered once by phase 02's composition root
/// after the shell window exists — this type only ever depends on the manager abstraction, keeping
/// this project window-agnostic (DIR-07, AT-08).
/// </summary>
public sealed class ToastService : IToastService
{
    private readonly INotificationManager _notificationManager;

    public ToastService(INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }

    public void ShowInfo(string message, string? title = null) => Show(title, message, NotificationType.Information);

    public void ShowSuccess(string message, string? title = null) => Show(title, message, NotificationType.Success);

    public void ShowWarning(string message, string? title = null) => Show(title, message, NotificationType.Warning);

    public void ShowError(string message, string? title = null) => Show(title, message, NotificationType.Error);

    private void Show(string? title, string message, NotificationType type) =>
        _notificationManager.Show(new Notification(title, message, type));
}
