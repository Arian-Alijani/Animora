using System.Reflection;
using Animora.SharedKernel.Primitives;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Animora.Desktop.ArchTests;

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
    public void SyncedEntitiesExposeNoPublicPrimaryKeySetter()
    {
        IEnumerable<Type> syncedEntities = DesktopAssemblies.All()
            .SelectMany(DesktopAssemblies.TypesIn)
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISyncedEntity).IsAssignableFrom(type));

        List<string> violations = [.. syncedEntities.Where(HasPublicIdSetter).Select(type => type.FullName ?? type.Name)];

        violations.Should().BeEmpty(
            "AT-07 is violated by: {0}",
            string.Join(", ", violations));
    }

    // An `init` accessor counts as a public setter here: it is still `set_Id` in IL, and on a record
    // a `with` expression would re-key a replicated row through it (CONV-03). A synced entity takes
    // its id through its constructor instead.
    private static bool HasPublicIdSetter(Type type)
    {
        // Enumerated rather than looked up by name: a derived entity that hides its base Id makes
        // GetProperty ambiguous, and both declarations have to be checked anyway. A type that
        // implements IEntity.Id explicitly exposes no public accessor here at all, which is the
        // rule's intent rather than a gap in it.
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.Name == nameof(IEntity.Id))
            .Any(property => property.SetMethod is { IsPublic: true });
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
