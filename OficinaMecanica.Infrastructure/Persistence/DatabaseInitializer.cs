using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Identity;

namespace OficinaMecanica.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    OficinaDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (context.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }
        await SeedIdentityAsync();
        await SeedDomainAsync(cancellationToken);
    }

    private async Task SeedIdentityAsync()
    {
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var username = configuration["InitialAdmin:Username"] ?? "admin";
        var email = configuration["InitialAdmin:Email"] ?? "admin@oficina.local";
        var password = configuration["InitialAdmin:Password"] ?? "Admin@123";
        var admin = await userManager.FindByNameAsync(username);

        if (admin is null)
        {
            admin = new AppUser { UserName = username, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar admin inicial: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private async Task SeedDomainAsync(CancellationToken cancellationToken)
    {
        if (await context.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        context.Customers.Add(customer);
        context.Vehicles.Add(new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020));
        context.ServicesCatalog.AddRange(
            new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m),
            new ServiceCatalogItem("Alinhamento", "Alinhamento e balanceamento", 120m, 1m));
        context.Parts.Add(new Part("Filtro de oleo", "FLT-001", 35m, 20));
        await context.SaveChangesAsync(cancellationToken);
    }
}

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(cancellationToken);
    }
}
