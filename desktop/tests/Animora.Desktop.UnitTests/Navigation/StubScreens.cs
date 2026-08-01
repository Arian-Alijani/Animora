using Animora.Desktop.UI.Mvvm;
using Animora.Desktop.UI.Navigation;
using Avalonia.Controls;

namespace Animora.Desktop.UnitTests.Navigation;

// The doubles below are public rather than internal because every one of them is reached only as a
// generic type argument (RouteDescriptor.Create<TViewModel, TView>) or through the container, which
// the unused-internal-type analyzer cannot see.

/// <summary>
/// A module screen's ViewModel as the navigation tests need it: derived from
/// <see cref="ViewModelBase"/> like every real screen, and <see cref="INavigationAware"/> so the
/// parameter hand-off can be observed. It records calls instead of substituting the interface because
/// <c>RouteDescriptor</c> resolves a concrete <see cref="ViewModelBase"/> from the container — a
/// substitute would prove the mock framework works, not that the container was used.
/// </summary>
public sealed class StubViewModel : ViewModelBase, INavigationAware
{
    public int NavigatedToCount { get; private set; }

    public object? NavigatedParameter { get; private set; }

    public void OnNavigatedTo(object? parameter)
    {
        NavigatedToCount++;
        NavigatedParameter = parameter;
    }
}

/// <summary>
/// The other half of <c>INavigationAware</c> being opt-in: a screen that takes no navigation
/// parameter implements nothing extra and must still navigate.
/// </summary>
public sealed class PlainViewModel : ViewModelBase
{
}

/// <summary>
/// Stands in for a screen's View. A bare <see cref="Control"/> is deliberate: the navigation service
/// only forwards the control the descriptor built, so nothing here needs a template, a style or an
/// Avalonia application session.
/// </summary>
public sealed class StubView : Control
{
}
