using Animora.Desktop.App.Startup;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Animora.Desktop.App;

public partial class App : Application
{
    private readonly StartupSequence? _startup;

    // Null on the design-time path only (Program.BuildAvaloniaApp()), where styles are all the
    // previewer needs.
    internal App(StartupSequence? startup)
    {
        _startup = startup;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (_startup is not null && ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The startup sequence owns stage order; this method only hands the window it produced to
            // the lifetime that shows it (DESK-ARCH-16).
            desktop.MainWindow = _startup.CreateShell();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
