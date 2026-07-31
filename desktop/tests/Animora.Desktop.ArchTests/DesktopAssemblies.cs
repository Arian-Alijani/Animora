using System.Reflection;

namespace Animora.Desktop.ArchTests;

internal static class DesktopAssemblies
{
    internal const string App = "Animora.Desktop.App";
    internal const string Ui = "Animora.Desktop.UI";
    internal const string Data = "Animora.Desktop.Data";
    internal const string Sync = "Animora.Desktop.Sync";
    internal const string Infrastructure = "Animora.Desktop.Infrastructure";

    internal const string ModulePrefix = "Animora.Desktop.Modules.";

    internal static readonly string[] ModuleNames =
    [
        ModulePrefix + "Identity",
        ModulePrefix + "Clients",
        ModulePrefix + "Visits",
        ModulePrefix + "Scheduling",
        ModulePrefix + "Finance",
        ModulePrefix + "Reporting",
        ModulePrefix + "Notifications",
        ModulePrefix + "Licensing",
        ModulePrefix + "Files",
    ];

    // Loaded by path rather than typeof(...) because most desktop assemblies legitimately hold zero
    // types until their phase lands, which leaves no anchor type to reference.
    internal static Assembly Load(string simpleName)
    {
        return Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, simpleName + ".dll"));
    }
}
