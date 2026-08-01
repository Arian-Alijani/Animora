using Animora.Desktop.Modules.Identity.Handlers;
using Animora.Desktop.UI.AppState;
using CommunityToolkit.Mvvm.ComponentModel;
using Mediator;

namespace Animora.Desktop.App.AppState;

/// <summary>
/// <see cref="ICurrentUserState"/> filled by <see cref="StaffSignedInNotification"/> (item 33):
/// <see cref="Handle"/> is this type's own Mediator notification handler, discovered the same way
/// every other handler in a referenced module assembly is (DESK-ARCH-03) — no separate handler class
/// is needed for a projection this small.
/// <para>
/// Until the first sign-in, the properties below still carry phase-02's fixed Persian placeholder,
/// because the login screen is rail-visible rather than gating the shell (PHASE 04's six-decision
/// note): the top bar's user chip must render something coherent before any credential is submitted.
/// </para>
/// </summary>
public sealed class CurrentUserState : ObservableObject, ICurrentUserState, INotificationHandler<StaffSignedInNotification>
{
    private bool _isSignedIn = true;
    private string _displayName = "دکتر سارا محمدی";
    private string _initial = "س";
    private string _roleDisplayName = "مدیر کلینیک";

    /// <inheritdoc />
    public bool IsSignedIn
    {
        get => _isSignedIn;
        private set => SetProperty(ref _isSignedIn, value);
    }

    /// <inheritdoc />
    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    /// <inheritdoc />
    public string Initial
    {
        get => _initial;
        private set => SetProperty(ref _initial, value);
    }

    /// <inheritdoc />
    public string RoleDisplayName
    {
        get => _roleDisplayName;
        private set => SetProperty(ref _roleDisplayName, value);
    }

    // No tenant-switch data exists yet (single-tenant Stage A demo), so this member has nothing to
    // project and stays the fixed placeholder every other property carried before item 33.
    /// <inheritdoc />
    public string TenantDisplayName => "کلینیک دامپزشکی آنیمورا";

    /// <summary>
    /// Projects a successful sign-in onto the properties above (DT-01, DESK-ARCH-03). Not awaited by
    /// its publisher beyond this call completing — there is nothing asynchronous to do once the
    /// notification's staff projection is already in hand.
    /// </summary>
    public ValueTask Handle(StaffSignedInNotification notification, CancellationToken cancellationToken)
    {
        var staff = notification.Staff;

        IsSignedIn = true;
        DisplayName = staff.FullName;
        // First character only, matching the avatar tile's single-glyph slot (ICurrentUserState.Initial);
        // no title-stripping (e.g. "دکتر") is applied — a phase-04 default, open to correction once a
        // real display-name formatting rule is specified (AG-02).
        Initial = staff.FullName.Length > 0 ? staff.FullName[..1] : string.Empty;
        RoleDisplayName = staff.RoleDisplayName;

        return ValueTask.CompletedTask;
    }
}
