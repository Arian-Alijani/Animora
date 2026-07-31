using Avalonia;
using Microsoft.Extensions.Hosting;

namespace Animora.Desktop.App.Startup;

internal static class Program
{
    // STA is required before any Avalonia/Win32 interop touches the UI thread.
    [STAThread]
    public static int Main(string[] args)
    {
        // The host owns background services and (from phase 02) the module service registrations;
        // the Avalonia lifetime runs inside it so shutdown order stays host-then-UI.
        using IHost host = Host.CreateApplicationBuilder(args).Build();
        host.Start();

        // TODO(P1-02): register modules, INavigationService, and the shell view models here.
        try
        {
            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
        }
    }

    // Avalonia's design-time tooling resolves this member by name and signature.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect();
    }
}
