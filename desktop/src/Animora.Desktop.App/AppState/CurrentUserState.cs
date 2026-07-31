using Animora.Desktop.UI.AppState;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Animora.Desktop.App.AppState;

/// <summary>
/// Phase-02 stand-in for <see cref="ICurrentUserState"/>: a fixed signed-in user so the shell's user
/// chip renders real Persian text before an authentication flow exists (DT-12 — no fake API, local
/// values only).
/// <para>
/// Deriving from <see cref="ObservableObject"/> now means the swap below only turns these constants
/// into <c>SetProperty</c>-backed properties; nothing bound to this instance has to change.
/// </para>
/// </summary>
// TODO(P1-04): project the restored Identity session onto these properties and raise
// PropertyChanged on sign-in/sign-out and clinic switch.
public sealed class CurrentUserState : ObservableObject, ICurrentUserState
{
    /// <inheritdoc />
    public bool IsSignedIn => true;

    /// <inheritdoc />
    public string DisplayName => "دکتر سارا محمدی";

    /// <inheritdoc />
    public string Initial => "س";

    /// <inheritdoc />
    public string RoleDisplayName => "مدیر کلینیک";

    /// <inheritdoc />
    public string TenantDisplayName => "کلینیک دامپزشکی آنیمورا";
}
