using System.ComponentModel;

namespace Animora.Desktop.UI.AppState;

/// <summary>
/// The signed-in user as the UI needs to display them (DESK-ARCH-03): one injected singleton any
/// screen may take, which is what keeps this state off <c>ViewModelBase</c>. Display labels only —
/// permission checks are a separate concern and belong to the Identity module's contract, so a screen
/// can never accidentally authorize itself from a label.
/// <para>
/// <see cref="INotifyPropertyChanged"/> is part of the contract because sign-in, profile edits and
/// clinic switching all mutate the same instance while views are bound to it.
/// </para>
/// </summary>
public interface ICurrentUserState : INotifyPropertyChanged
{
    /// <summary>Whether a session is currently restored; <see langword="false"/> shows the sign-in flow.</summary>
    bool IsSignedIn { get; }

    /// <summary>Full Persian display name for the top bar's user chip (design-reference.md §6).</summary>
    string DisplayName { get; }

    /// <summary>
    /// Single leading character of <see cref="DisplayName"/> for the avatar tile, supplied here rather
    /// than derived in a converter so RTL/Persian edge cases have exactly one owner.
    /// </summary>
    string Initial { get; }

    /// <summary>Display name of the user's tenant-defined role (SEC-09: roles are named bundles).</summary>
    string RoleDisplayName { get; }

    /// <summary>Display name of the active tenant/clinic, shown next to the brand mark.</summary>
    string TenantDisplayName { get; }
}
