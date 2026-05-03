using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions.Repositories;

public interface IPartRepository
{
    Task<IReadOnlyCollection<Part>> GetAllAsync(CancellationToken cancellationToken);
    Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Part>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    void Add(Part part);
    void Remove(Part part);
}
