using Animora.Desktop.UI.Services;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Animora.Desktop.App;

public partial class App : Application
{
    public override void Initialize()
    {
        // TODO(P1-02): drop this direct call once the composition root resolves the design system
        // through Animora.Desktop.UI.Services.ServiceCollectionExtensions.AddDesktopUi().
        IconProviderRegistrar.Register();

        AvaloniaXamlLoader.Load(this);
    }

    // TODO(P1-02): assign MainWindow from the host container here — ShellWindow now takes its
    // ShellViewModel by constructor injection, so the startup sequence resolves the window instead of
    // this method newing it up (DESK-ARCH-05/16).
}
