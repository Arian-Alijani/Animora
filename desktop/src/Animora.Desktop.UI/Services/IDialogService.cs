namespace Animora.Desktop.UI.Services;

/// <summary>
/// Abstraction over <c>DialogHost.Avalonia</c> (TECH_STACK §4) so a module ViewModel can show a
/// dialog and await its result without a static dependency on
/// <c>DialogHostAvalonia.DialogHost</c> and without this service's own signature ever naming a
/// module type (DIR-07). <c>content</c> is typically a ViewModel; resolving it to the actual View
/// is the app's <c>DataTemplate</c> concern (registered per module at composition), not this
/// service's.
/// </summary>
public interface IDialogService
{
    /// <summary>Opens <paramref name="content"/> in the dialog host identified by
    /// <paramref name="dialogHostIdentifier"/> (or the app's sole host when
    /// <see langword="null"/>) and completes once the dialog is closed, with whatever value it was
    /// closed with (typically via <c>DialogHost.CloseDialogCommand</c>'s parameter).</summary>
    Task<object?> ShowAsync(object content, string? dialogHostIdentifier = null);

    /// <summary>Closes the current dialog on the host identified by
    /// <paramref name="dialogHostIdentifier"/>, completing the pending <see cref="ShowAsync"/>
    /// task with <paramref name="result"/>. For dialogs that close themselves via
    /// <c>DialogHost.CloseDialogCommand</c> this is only needed for programmatic dismissal (e.g. a
    /// timeout or an external cancellation).</summary>
    void Close(string? dialogHostIdentifier = null, object? result = null);

    /// <summary>Whether a dialog is currently open on the host identified by
    /// <paramref name="dialogHostIdentifier"/>.</summary>
    bool IsOpen(string? dialogHostIdentifier = null);
}
