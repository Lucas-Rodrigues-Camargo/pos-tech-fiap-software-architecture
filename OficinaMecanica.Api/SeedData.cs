using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Identity;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Api;

public static class SeedData
{
    public static async Task InitializeAsync(
        OficinaDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUsername = configuration["InitialAdmin:Username"] ?? "admin";
        var adminEmail = configuration["InitialAdmin:Email"] ?? "admin@oficina.local";
        var adminPassword = configuration["InitialAdmin:Password"] ?? "Admin@123";

        var admin = await userManager.FindByNameAsync(adminUsername);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar admin inicial: {string.Join("; ", result.Errors.Select(x => x.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await context.Customers.AnyAsync())
        {
            return;
        }

        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var oilChange = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var alignment = new ServiceCatalogItem("Alinhamento", "Alinhamento e balanceamento", 120m, 1m);
        var oilFilter = new Part("Filtro de oleo", "FLT-001", 35m, 20);

        context.Customers.Add(customer);
        context.Vehicles.Add(vehicle);
        context.ServicesCatalog.AddRange(oilChange, alignment);
        context.Parts.Add(oilFilter);

        await context.SaveChangesAsync();
    }
}
