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

    internal const string SharedKernel = "Animora.SharedKernel";
    internal const string Contracts = "Animora.Contracts";

    // Loaded by path rather than typeof(...) because most desktop assemblies legitimately hold zero
    // types until their phase lands, which leaves no anchor type to reference.
    internal static Assembly Load(string simpleName)
    {
        return Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, simpleName + ".dll"));
    }

    // Entity classes may land in any desktop or shared assembly as the module phases arrive, so
    // rules that sweep for a marker interface (AT-07) discover their input from the output folder
    // instead of a hand-maintained list that would silently miss the next module's entities.
    internal static IReadOnlyList<Assembly> All()
    {
        return [.. Directory.EnumerateFiles(AppContext.BaseDirectory, "Animora.*.dll")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(Assembly.LoadFrom)];
    }

    // A partially loadable assembly still yields the types that did load; failing the whole sweep
    // because one unrelated type could not be resolved would turn a boundary rule into a flake.
    internal static IEnumerable<Type> TypesIn(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>();
        }
    }
}
