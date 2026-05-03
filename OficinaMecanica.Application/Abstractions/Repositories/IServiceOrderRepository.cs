using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions.Repositories;

public interface IServiceOrderRepository
{
    Task<IReadOnlyCollection<ServiceOrder>> GetAllWithDetailsAsync(CancellationToken cancellationToken);
    Task<ServiceOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceOrder?> GetByNumberAndCustomerDocumentAsync(string number, string normalizedDocument, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ServiceOrder>> GetAllAsync(CancellationToken cancellationToken);
    void Add(ServiceOrder order);
}
