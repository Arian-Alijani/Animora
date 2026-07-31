using CommunityToolkit.Mvvm.ComponentModel;

namespace Animora.Desktop.UI.Mvvm;

/// <summary>
/// Base type for every screen/dialog ViewModel (DESK-ARCH-01: Views bind to ViewModels only, built
/// on <c>CommunityToolkit.Mvvm</c>). Deliberately carries no properties or behaviour of its own —
/// cross-screen state (current user, connectivity/sync status, entitlement snapshot) belongs in a
/// small set of injected singleton "app state" services instead, never here, so this stays a thin
/// MVVM primitive rather than the shared base ViewModel god-object DESK-ARCH-03 rules out. Deriving
/// from <see cref="ObservableObject"/> gives every ViewModel <c>SetProperty</c>/<c>OnPropertyChanged</c>
/// and (via the source generator) the <c>[ObservableProperty]</c>/<c>[RelayCommand]</c> attributes.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
