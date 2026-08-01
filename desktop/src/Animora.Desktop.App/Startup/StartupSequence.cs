using Animora.Desktop.App.Composition;
using Animora.Desktop.App.Shell;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Animora.Desktop.App.Startup;

/// <summary>
/// DESK-ARCH-16's startup stages as one readable order: host bootstrap -> local database open ->
/// shell shown -> background init. Each stage is a member of this type rather than a step spread
/// over <c>Program</c> and <c>App</c>, so the phases that fill the two placeholder stages change one
/// method and cannot accidentally move a stage in front of the shell.
/// <para>
/// The deferred work is deliberately started, never awaited: nothing between
/// <see cref="Bootstrap"/> and <see cref="CreateShell"/> may wait on the network, a probe, or a job
/// scheduler (DESK-ARCH-16, INV-15).
/// </para>
/// </summary>
internal sealed class StartupSequence : IDisposable
{
    private readonly IHost _host;

    private StartupSequence(IHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Stage 1 — host bootstrap: builds the container and starts the host's hosted services before
    /// Avalonia exists, so no view can be constructed against a half-built container.
    /// </summary>
    internal static StartupSequence Bootstrap(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddDesktopApp();

        IHost host = builder.Build();

        // Synchronous by choice: the UI thread owns startup, and Start() only runs hosted services,
        // of which there are none until the background-init stage below gains an owner.
        host.Start();

        return new StartupSequence(host);
    }

    /// <summary>
    /// Stages 2-4. Returns the shell window for the desktop lifetime to show, which is why the
    /// background-init stage is started here and not after the first frame: the caller assigns the
    /// window and returns, and no code path in between is allowed to block on init.
    /// </summary>
    internal Window CreateShell()
    {
        OpenLocalDatabase();

        ShellWindow shell = _host.Services.GetRequiredService<ShellWindow>();

        // Creating the toast manager now attaches it to this window's adorner layer at a deterministic
        // point instead of on whichever screen raises the first toast (IToastService, DT-10).
        _ = _host.Services.GetRequiredService<INotificationManager>();

        // Landing route before the window is shown, so the content region is never empty on first
        // paint; the shell resolves it from the registry, so this names no module screen (DT-09).
        _host.Services.GetRequiredService<ShellViewModel>().ShowDefaultRoute();

        StartBackgroundInit();

        return shell;
    }

    public void Dispose()
    {
        // Host-then-UI shutdown order: the caller disposes this after the Avalonia lifetime returns.
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }

    // TODO(P1-14): open the SQLCipher connection and apply pending desktop migrations here, after a
    // pre-migration snapshot (DT-11). Failing this stage must fail the launch — the shell has no
    // meaning without local data — which is why it sits before the window is resolved.
    private static void OpenLocalDatabase()
    {
    }

    // TODO(P1-26): start the local job scheduler here (and, in P2, the connectivity probe and sync
    // engine) as fire-and-forget work. Whatever lands here must not be awaited by CreateShell.
    private static void StartBackgroundInit()
    {
    }
}
