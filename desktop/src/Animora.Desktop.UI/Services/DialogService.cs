using DialogHostAvalonia;

namespace Animora.Desktop.UI.Services;

/// <summary>
/// <see cref="IDialogService"/> over <c>DialogHost.Avalonia</c>'s static <see cref="DialogHost"/>
/// API. Holds no dialog-specific logic of its own — every member is a direct pass-through, so
/// callers get the seam (testability, no static coupling in a ViewModel) without a second copy of
/// <c>DialogHost</c>'s behaviour to keep in sync.
/// </summary>
public sealed class DialogService : IDialogService
{
    public Task<object?> ShowAsync(object content, string? dialogHostIdentifier = null) =>
        DialogHost.Show(content, dialogHostIdentifier);

    public void Close(string? dialogHostIdentifier = null, object? result = null) =>
        DialogHost.Close(dialogHostIdentifier, result);

    public bool IsOpen(string? dialogHostIdentifier = null) => DialogHost.IsDialogOpen(dialogHostIdentifier);
}
