using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Tests.Integration;

internal sealed class TestDatabase : IAsyncDisposable
{
    private TestDatabase(OficinaDbContext context)
    {
        Context = context;
    }

    public OficinaDbContext Context { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var database = new TestDatabase(
            new OficinaDbContext(
                new DbContextOptionsBuilder<OficinaDbContext>()
                    .UseSqlite("Data Source=:memory:")
                    .Options));

        await database.Context.Database.OpenConnectionAsync();
        await database.Context.Database.EnsureCreatedAsync();
        return database;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }
}
