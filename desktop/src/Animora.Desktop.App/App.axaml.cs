using Animora.Desktop.App.Gallery;
using Animora.Desktop.UI.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Animora.Desktop.App;

public partial class App : Application
{
    public override void Initialize()
    {
        // TODO(P1-01): registered here only so the throwaway token gallery window (item 35) can
        // render `{i:Icon ...}` glyphs; phase 02's composition root registers this via
        // Animora.Desktop.UI.Services.ServiceCollectionExtensions.AddDesktopUi() instead.
        IconProviderRegistrar.Register();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // TODO(P1-01): shows the phase-01 token gallery (TODO.md item 35) instead of
            // ShellWindow; revert to `new ShellWindow()` after the phase-01 host check (item 37).
            desktop.MainWindow = new TokenGalleryWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
