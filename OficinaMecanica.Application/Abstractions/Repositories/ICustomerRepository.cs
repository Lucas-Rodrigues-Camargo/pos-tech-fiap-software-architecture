using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken);
    Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken);
    Task<bool> ExistsByDocumentExcludingIdAsync(Guid id, string document, CancellationToken cancellationToken);
    void Add(Customer customer);
    void Remove(Customer customer);
}
