using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Animora.Desktop.ArchTests;

// DESK-ARCH-05/DT-09 inside the composition root, which AssemblyBoundaryRules cannot express: the App
// project legitimately references every module, so the boundary being guarded here is a namespace one.
// Shell/ and Navigation/ must reach module screens only through the descriptors modules register, while
// Composition/ is the one namespace allowed to name a module (its AddReportingModule call). The rule is
// deliberately scoped to those two namespaces rather than written as "every namespace except
// Composition": Mediator's generated registrations land outside Composition and correctly name module
// handler types, which is not a shell coupling.
public class ShellDecouplingRules
{
    [Theory]
    [InlineData(DesktopAssemblies.App + ".Shell")]
    [InlineData(DesktopAssemblies.App + ".Navigation")]
    public void ShellAndNavigationTypesDependOnNoModuleNamespace(string namespaceName)
    {
        Assembly app = DesktopAssemblies.Load(DesktopAssemblies.App);

        Types.InAssembly(app)
            .That()
            .ResideInNamespace(namespaceName)
            .ShouldNot()
            .HaveDependencyOn(DesktopAssemblies.ModulePrefix)
            .ShouldPass("DESK-ARCH-05");
    }
}
