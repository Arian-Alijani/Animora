using Avalonia.Controls;

namespace Animora.Desktop.App.Shell;

public partial class ShellWindow : Window
{
    // Resolved from the container with its view model rather than constructed anywhere by hand, so the
    // shell reaches module screens only through the registry and navigation service (DESK-ARCH-05).
    // No code-behind beyond this: the rail, top bar and content region are bindings, not procedures.
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
