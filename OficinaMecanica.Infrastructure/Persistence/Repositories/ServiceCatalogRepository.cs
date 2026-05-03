using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class ServiceCatalogRepository(OficinaDbContext context) : IServiceCatalogRepository
{
    public async Task<IReadOnlyCollection<ServiceCatalogItem>> GetAllAsync(CancellationToken cancellationToken)
        => await context.ServicesCatalog.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<ServiceCatalogItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.ServicesCatalog.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyCollection<ServiceCatalogItem>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => await context.ServicesCatalog.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public void Add(ServiceCatalogItem item) => context.ServicesCatalog.Add(item);
    public void Remove(ServiceCatalogItem item) => context.ServicesCatalog.Remove(item);
}
