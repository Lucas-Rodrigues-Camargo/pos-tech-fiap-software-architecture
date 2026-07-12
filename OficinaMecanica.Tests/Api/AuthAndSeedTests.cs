using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.Api.Auth;
using OficinaMecanica.Api.Controllers;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Contracts.Auth;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Infrastructure.Identity;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Tests.Api;

public sealed class AuthAndSeedTests
{
    [Fact]
    public async Task AuthControllerDeveRegistrarELogarUsuario()
    {
        await using var services = await CreateIdentityServicesAsync();
        var controller = CreateController(services);

        var register = await controller.Register(new RegisterRequest("maria", "maria@oficina.local", "Admin@1234", false));
        var login = await controller.Login(new LoginRequest("maria", "Admin@1234"));

        var created = Assert.IsType<CreatedResult>(register);
        var ok = Assert.IsType<OkObjectResult>(login);
        var token = Assert.IsType<OficinaMecanica.Application.Contracts.Auth.LoginResponse>(ok.Value);

        Assert.Equal(201, created.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
    }

    [Fact]
    public async Task AuthControllerDeveBloquearUsuarioDuplicadoECredenciaisInvalidas()
    {
        await using var services = await CreateIdentityServicesAsync();
        var controller = CreateController(services);

        await controller.Register(new RegisterRequest("maria", "maria@oficina.local", "Admin@1234", false));

        var duplicated = await controller.Register(new RegisterRequest("maria", "outra@oficina.local", "Admin@1234", false));
        var missingUser = await controller.Login(new LoginRequest("nao-existe", "Admin@1234"));
        var wrongPassword = await controller.Login(new LoginRequest("maria", "Senha@123"));

        Assert.IsType<ConflictObjectResult>(duplicated);
        Assert.IsType<UnauthorizedObjectResult>(missingUser);
        Assert.IsType<UnauthorizedObjectResult>(wrongPassword);
    }

    [Fact]
    public async Task SeedDataDeveCriarPerfisAdminEDadosBasicos()
    {
        await using var services = await CreateIdentityServicesAsync();
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var initializer = new DatabaseInitializer(context, users, roles, configuration);
        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        var admin = await users.FindByNameAsync("admin");

        Assert.True(await roles.RoleExistsAsync("Admin"));
        Assert.True(await roles.RoleExistsAsync("User"));
        Assert.NotNull(admin);
        Assert.True(await users.IsInRoleAsync(admin!, "Admin"));
        Assert.Single(context.Customers);
        Assert.Single(context.Parts);
        Assert.Equal(2, await context.ServicesCatalog.CountAsync());
    }

    [Fact]
    public void AddInfrastructureDeveRegistrarRepositorios()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);

        Assert.Contains(services, x => x.ServiceType == typeof(ICustomerRepository));
        Assert.Contains(services, x => x.ServiceType == typeof(IVehicleRepository));
        Assert.Contains(services, x => x.ServiceType == typeof(IServiceCatalogRepository));
        Assert.Contains(services, x => x.ServiceType == typeof(IPartRepository));
        Assert.Contains(services, x => x.ServiceType == typeof(IServiceOrderRepository));
    }

    private static AuthController CreateController(IServiceProvider provider)
        => new(
            provider.GetRequiredService<UserManager<AppUser>>(),
            provider.GetRequiredService<SignInManager<AppUser>>(),
            provider.GetRequiredService<JwtTokenService>());

    private static async Task<ServiceProvider> CreateIdentityServicesAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-dev-only-2026",
                ["Jwt:Issuer"] = "OficinaMecanica",
                ["Jwt:Audience"] = "OficinaMecanica.Admin"
            })
            .Build();

        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<JwtTokenService>();
        services.AddDbContext<OficinaDbContext>(options => options.UseSqlite(connection));
        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<OficinaDbContext>()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<OficinaDbContext>().Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole("Admin"));
        await roles.CreateAsync(new IdentityRole("User"));
        return provider;
    }
}
