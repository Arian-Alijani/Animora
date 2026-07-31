using System.Xml.Linq;

namespace Animora.Desktop.ArchTests;

// Assembly metadata cannot answer "which projects does this project reference": the compiler prunes
// references whose types are never used, so an empty or thin assembly reports none of them. The
// project files are therefore the only faithful source for the DIR-01/DIR-05/DIR-07 direction check.
internal static class DesktopProjects
{
    internal static IReadOnlyList<string> ReferencedDesktopProjects(string projectName)
    {
        XDocument project = XDocument.Load(ProjectPath(projectName));

        return project.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => include is not null)
            .Select(include => Path.GetFileNameWithoutExtension(include!))
            .Where(name => name.StartsWith("Animora.Desktop.", StringComparison.Ordinal))
            .ToList();
    }

    private static string ProjectPath(string projectName)
    {
        return Path.Combine(SourceRoot(), projectName, projectName + ".csproj");
    }

    private static string SourceRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src");
            if (File.Exists(Path.Combine(directory.FullName, "Animora.Desktop.sln")) && Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("desktop/src was not found above " + AppContext.BaseDirectory);
    }
}
