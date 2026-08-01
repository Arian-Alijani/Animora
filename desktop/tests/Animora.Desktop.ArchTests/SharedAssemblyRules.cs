using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Animora.Desktop.ArchTests;

// SH-01/SH-02 and INV-05 for shared/dotnet. The rules live in the desktop arch-test project because
// it is the only test project that builds in P1 (AG-01 keeps backend/ empty); they move to a shared
// arch-test project when P2 adds one. Both project-file and compiled-assembly evidence is checked:
// the project file catches a reference nobody uses yet, the assembly catches a dependency that
// arrives transitively through a package.
public class SharedAssemblyRules
{
    // A shared assembly that gains any of these has stopped being platform-neutral (SH-02); EF Core
    // and Npgsql additionally pull persistence into a contract assembly (AT-02).
    private static readonly string[] PlatformDependencies =
    [
        "Avalonia",
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.Sqlite",
        "Npgsql",
    ];

    public static TheoryData<string> SharedAssemblyNames =>
        new([DesktopAssemblies.SharedKernel, DesktopAssemblies.Contracts]);

    [Theory]
    [MemberData(nameof(SharedAssemblyNames))]
    public void SharedProjectsReferenceNoOtherProject(string projectName)
    {
        IReadOnlyList<string> references = DesktopProjects.ReferencedProjects(DesktopProjects.SharedProjectPath(projectName));

        references.Should().BeEmpty("SH-01/AT-02: {0} is a leaf assembly", projectName);
    }

    [Theory]
    [MemberData(nameof(SharedAssemblyNames))]
    public void SharedProjectsReferenceNoPlatformPackage(string projectName)
    {
        IEnumerable<string> platformPackages = DesktopProjects
            .ReferencedPackages(DesktopProjects.SharedProjectPath(projectName))
            .Where(IsPlatformDependency);

        platformPackages.Should().BeEmpty("SH-02: {0} must stay platform-neutral", projectName);
    }

    [Theory]
    [MemberData(nameof(SharedAssemblyNames))]
    public void SharedTypesDependOnNoModuleOrPlatformAssembly(string assemblyName)
    {
        Assembly shared = DesktopAssemblies.Load(assemblyName);
        string[] forbidden = ["Animora.Desktop.", "Animora.Backend.", .. PlatformDependencies];

        Types.InAssembly(shared)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .ShouldPass("SH-01/SH-02/AT-02");
    }

    [Fact]
    public void SharedSourceHasNoPlatformConditionalCompilation()
    {
        // SH-02's "#if WINDOWS" half: a preprocessor branch is invisible in the compiled assembly
        // (only one side of it is emitted), so the source is the only place this can be checked —
        // the same reason TokenDisciplineRules reads .axaml directly.
        List<string> violations =
        [
            .. SharedSourceFiles()
                .Where(path => File.ReadLines(path).Any(line => line.TrimStart().StartsWith("#if", StringComparison.Ordinal)))
                .Select(path => Path.GetRelativePath(DesktopProjects.SharedSourceRoot(), path)),
        ];

        violations.Should().BeEmpty("SH-02 forbids platform-conditional code in shared/dotnet: {0}", string.Join(", ", violations));
    }

    [Theory]
    [MemberData(nameof(SharedAssemblyNames))]
    public void PublicSharedTypesExposeNoBinaryFloatingPointMember(string assemblyName)
    {
        // INV-05: money is decimal(18,2) end to end. The rule is written against the whole public
        // surface rather than against Money alone, because the way a float reaches a ledger is a
        // percentage or a quantity on some other shared type, not the money type itself.
        Assembly shared = DesktopAssemblies.Load(assemblyName);

        List<string> violations = [.. shared.GetExportedTypes().SelectMany(FloatingPointMembers)];

        violations.Should().BeEmpty("INV-05 is violated by: {0}", string.Join(", ", violations));
    }

    private static IEnumerable<string> FloatingPointMembers(Type type)
    {
        const BindingFlags Declared =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (FieldInfo field in type.GetFields(Declared).Where(field => IsBinaryFloatingPoint(field.FieldType)))
        {
            yield return $"{type.Name}.{field.Name}";
        }

        foreach (PropertyInfo property in type.GetProperties(Declared).Where(property => IsBinaryFloatingPoint(property.PropertyType)))
        {
            yield return $"{type.Name}.{property.Name}";
        }

        foreach (MethodBase method in type.GetMethods(Declared).Concat<MethodBase>(type.GetConstructors(Declared)))
        {
            if (method is MethodInfo function && IsBinaryFloatingPoint(function.ReturnType))
            {
                yield return $"{type.Name}.{method.Name}() returns {function.ReturnType.Name}";
            }

            foreach (ParameterInfo parameter in method.GetParameters().Where(parameter => IsBinaryFloatingPoint(parameter.ParameterType)))
            {
                yield return $"{type.Name}.{method.Name}({parameter.Name})";
            }
        }
    }

    private static bool IsBinaryFloatingPoint(Type type)
    {
        if (type.HasElementType)
        {
            Type? element = type.GetElementType();

            return element is not null && IsBinaryFloatingPoint(element);
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(Half))
        {
            return true;
        }

        // Covers Nullable<double>, IReadOnlyList<double> and every other wrapper a member could
        // smuggle a binary float through.
        return type.IsGenericType && type.GetGenericArguments().Any(IsBinaryFloatingPoint);
    }

    private static bool IsPlatformDependency(string packageId) =>
        PlatformDependencies.Any(forbidden => packageId.StartsWith(forbidden, StringComparison.Ordinal));

    private static IEnumerable<string> SharedSourceFiles()
    {
        return Directory.EnumerateFiles(DesktopProjects.SharedSourceRoot(), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "obj" or "bin"))
            .OrderBy(path => path, StringComparer.Ordinal);
    }
}
