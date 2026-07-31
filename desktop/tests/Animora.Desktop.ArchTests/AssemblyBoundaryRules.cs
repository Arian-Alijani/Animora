using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Animora.Desktop.ArchTests;

public class AssemblyBoundaryRules
{
    public static TheoryData<string> ModuleAssemblies => new(DesktopAssemblies.ModuleNames);

    [Fact]
    public void DesignSystemProjectReferencesNoOtherDesktopProject()
    {
        IReadOnlyList<string> references = DesktopProjects.ReferencedDesktopProjects(DesktopAssemblies.Ui);

        references.Should().BeEmpty("AT-08");
    }

    [Fact]
    public void DesignSystemTypesDependOnNoOtherDesktopNamespace()
    {
        Assembly ui = DesktopAssemblies.Load(DesktopAssemblies.Ui);
        string[] forbidden =
        [
            DesktopAssemblies.App,
            DesktopAssemblies.Data,
            DesktopAssemblies.Sync,
            DesktopAssemblies.Infrastructure,
            DesktopAssemblies.ModulePrefix,
        ];

        Types.InAssembly(ui)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .ShouldPass("AT-08");
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void ModuleProjectReferencesNoSiblingModuleAndNotTheCompositionRoot(string moduleName)
    {
        IEnumerable<string> forbidden = DesktopProjects.ReferencedDesktopProjects(moduleName)
            .Where(name => name == DesktopAssemblies.App
                || (name.StartsWith(DesktopAssemblies.ModulePrefix, StringComparison.Ordinal)
                    && name != moduleName));

        forbidden.Should().BeEmpty("AT-09");
    }

    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void ModuleTypesDependOnNoSiblingModuleAndNotTheCompositionRoot(string moduleName)
    {
        Assembly module = DesktopAssemblies.Load(moduleName);
        string[] forbidden =
        [
            DesktopAssemblies.App,
            .. DesktopAssemblies.ModuleNames.Where(name => name != moduleName),
        ];

        Types.InAssembly(module)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .ShouldPass("AT-09");
    }
}
