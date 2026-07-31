namespace Animora.Desktop.UI.Navigation;

/// <summary>
/// Opt-in hook for a ViewModel that needs the argument passed to
/// <see cref="INavigationService.NavigateTo"/>. Keeping it optional is what lets
/// <see cref="RouteDescriptor.CreateViewModel"/> stay a plain DI resolution: a screen that takes no
/// parameter implements nothing, and a screen that does still gets its dependencies injected normally
/// instead of through a navigation-specific constructor.
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// Called once per navigation, after the ViewModel is resolved and before its View is built, so
    /// property changes raised here are already applied when the View binds. Runs on the UI thread
    /// and must not block: a screen that needs to load data starts an async command instead
    /// (DESK-ARCH-10).
    /// </summary>
    void OnNavigatedTo(object? parameter);
}
