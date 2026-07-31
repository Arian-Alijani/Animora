using Animora.Desktop.App.Shell;
using Animora.Desktop.UI.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ShellWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
