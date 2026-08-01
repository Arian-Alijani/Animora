using System.Xml.Linq;

namespace Animora.Desktop.ArchTests;

// Assembly metadata cannot answer "which projects does this project reference": the compiler prunes
// references whose types are never used, so an empty or thin assembly reports none of them. The
// project files are therefore the only faithful source for the DIR-01/DIR-05/DIR-07 direction check.
internal static class DesktopProjects
{
    internal static IReadOnlyList<string> ReferencedDesktopProjects(string projectName)
    {
        return [.. ReferencedProjects(DesktopProjectPath(projectName))
            .Where(name => name.StartsWith("Animora.Desktop.", StringComparison.Ordinal))];
    }

    internal static IReadOnlyList<string> ReferencedProjects(string projectPath)
    {
        return [.. Includes(projectPath, "ProjectReference")
            .Select(include => Path.GetFileNameWithoutExtension(include))];
    }

    internal static IReadOnlyList<string> ReferencedPackages(string projectPath)
    {
        return [.. Includes(projectPath, "PackageReference")];
    }

    // shared/dotnet sits outside the desktop tree: SH-01's leaf-assembly check reads those project
    // files from the repository root, not from desktop/src.
    internal static string SharedProjectPath(string projectName)
    {
        return Path.Combine(RepositoryRoot(), "shared", "dotnet", projectName, projectName + ".csproj");
    }

    internal static string SharedSourceRoot() => Path.Combine(RepositoryRoot(), "shared", "dotnet");

    // Exposed for TokenDisciplineRules, which walks the same source tree to find .axaml files
    // rather than a project file (DT-06 is a markup-literal concern, not a reference-direction one).
    internal static string SourceRoot() => Path.Combine(SolutionDirectory(), "src");

    private static string DesktopProjectPath(string projectName)
    {
        return Path.Combine(SourceRoot(), projectName, projectName + ".csproj");
    }

    private static IEnumerable<string> Includes(string projectPath, string elementName)
    {
        XDocument project = XDocument.Load(projectPath);

        return project.Descendants(elementName)
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrEmpty(include))
            .Select(include => include!);
    }

    private static string RepositoryRoot()
    {
        return Directory.GetParent(SolutionDirectory())?.FullName
            ?? throw new DirectoryNotFoundException("the repository root was not found above " + SolutionDirectory());
    }

    private static string SolutionDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Animora.Desktop.sln"))
                && Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Animora.Desktop.sln was not found above " + AppContext.BaseDirectory);
    }
}
