using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions;

public interface IOficinaDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<ServiceCatalogItem> ServicesCatalog { get; }
    DbSet<Part> Parts { get; }
    DbSet<ServiceOrder> ServiceOrders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
