using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Animora.Desktop.ArchTests;

// TODO(P1-03): implement AT-07 (no public primary-key setter on ISyncedEntity implementations, INV-03)
// once SharedKernel declares ISyncedEntity.
public class PersistenceBoundaryRules
{
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string Npgsql = "Npgsql";
    private const string Dapper = "Dapper";

    // DIR-03 constrains modules and the design system only: the composition root wires DbContext
    // registrations and Sync drains the outbox, so both legitimately touch EF Core.
    public static TheoryData<string> ProviderFreeAssemblies =>
        new([.. DesktopAssemblies.ModuleNames, DesktopAssemblies.Ui]);

    [Theory]
    [MemberData(nameof(ProviderFreeAssemblies))]
    public void ModuleAndDesignSystemTypesDoNotDependOnADatabaseProvider(string assemblyName)
    {
        Assembly assembly = DesktopAssemblies.Load(assemblyName);

        Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(EntityFrameworkCore, Npgsql)
            .ShouldPass("AT-03");
    }

    [Fact]
    public void ReadQueryTypesDoNotDependOnEntityFrameworkCore()
    {
        Assembly data = DesktopAssemblies.Load(DesktopAssemblies.Data);

        Types.InAssembly(data)
            .That()
            .ResideInNamespace($"{DesktopAssemblies.Data}.Queries")
            .ShouldNot()
            .HaveDependencyOn(EntityFrameworkCore)
            .ShouldPass("AT-04");
    }

    [Fact]
    public void ReportingTypesDoNotDependOnEntityFrameworkCore()
    {
        Assembly reporting = DesktopAssemblies.Load($"{DesktopAssemblies.ModulePrefix}Reporting");

        Types.InAssembly(reporting)
            .ShouldNot()
            .HaveDependencyOn(EntityFrameworkCore)
            .ShouldPass("AT-04");
    }

    [Fact]
    public void WriteTypesDoNotDependOnDapper()
    {
        Assembly data = DesktopAssemblies.Load(DesktopAssemblies.Data);

        Types.InAssembly(data)
            .That()
            .ResideInNamespace($"{DesktopAssemblies.Data}.Writes")
            .ShouldNot()
            .HaveDependencyOn(Dapper)
            .ShouldPass("AT-05");
    }
}
