using System.Reflection;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Tests.Architecture;

public sealed class DependencyRuleTests
{
    [Fact]
    public void DomainMustNotDependOnOuterLayersOrFrameworks()
    {
        var references = ReferencesOf(typeof(ServiceOrder).Assembly);

        Assert.DoesNotContain("OficinaMecanica.Application", references);
        Assert.DoesNotContain("OficinaMecanica.Infrastructure", references);
        Assert.DoesNotContain("OficinaMecanica.Api", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void ApplicationMustDependOnlyOnDomainAndUseCaseLibraries()
    {
        var references = ReferencesOf(typeof(IUnitOfWork).Assembly);

        Assert.Contains("OficinaMecanica.Domain", references);
        Assert.DoesNotContain("OficinaMecanica.Infrastructure", references);
        Assert.DoesNotContain("OficinaMecanica.Api", references);
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void InfrastructureMustNotDependOnApi()
    {
        var references = ReferencesOf(typeof(OficinaDbContext).Assembly);

        Assert.Contains("OficinaMecanica.Application", references);
        Assert.Contains("OficinaMecanica.Domain", references);
        Assert.DoesNotContain("OficinaMecanica.Api", references);
    }

    private static IReadOnlyCollection<string> ReferencesOf(Assembly assembly)
        => assembly.GetReferencedAssemblies().Select(reference => reference.Name!).ToArray();
}
