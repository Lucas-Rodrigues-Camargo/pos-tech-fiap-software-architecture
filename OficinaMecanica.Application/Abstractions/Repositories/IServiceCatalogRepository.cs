using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions.Repositories;

public interface IServiceCatalogRepository
{
    Task<IReadOnlyCollection<ServiceCatalogItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<ServiceCatalogItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ServiceCatalogItem>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    void Add(ServiceCatalogItem item);
    void Remove(ServiceCatalogItem item);
}
