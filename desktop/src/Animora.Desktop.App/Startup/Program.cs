using Avalonia;

namespace Animora.Desktop.App.Startup;

internal static class Program
{
    // STA is required before any Avalonia/Win32 interop touches the UI thread.
    [STAThread]
    public static int Main(string[] args)
    {
        // Stage 1 completes before Avalonia starts, so App resolves an already-built container;
        // disposing stops the host once the UI loop returns (DESK-ARCH-16).
        using StartupSequence startup = StartupSequence.Bootstrap(args);

        return BuildAvaloniaApp(startup).StartWithClassicDesktopLifetime(args);
    }

    // Avalonia's design-time tooling resolves this member by name and signature. It passes no startup
    // sequence: the previewer renders XAML only, and bootstrapping the host would run the app's real
    // startup stages inside the designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return BuildAvaloniaApp(startup: null);
    }

    private static AppBuilder BuildAvaloniaApp(StartupSequence? startup)
    {
        return AppBuilder.Configure(() => new App(startup))
            .UsePlatformDetect();
    }
}
