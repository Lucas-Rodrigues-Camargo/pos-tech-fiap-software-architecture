using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class PartRepository(OficinaDbContext context) : IPartRepository
{
    public async Task<IReadOnlyCollection<Part>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Parts.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<Part?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Parts.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyCollection<Part>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => await context.Parts.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
        => await context.Parts.AnyAsync(x => x.Sku == sku, cancellationToken);

    public async Task<bool> ExistsBySkuExcludingIdAsync(Guid id, string sku, CancellationToken cancellationToken)
        => await context.Parts.AnyAsync(x => x.Id != id && x.Sku == sku, cancellationToken);

    public void Add(Part part) => context.Parts.Add(part);
    public void Remove(Part part) => context.Parts.Remove(part);
}
