using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class ServiceOrderRepository(OficinaDbContext context) : IServiceOrderRepository
{
    public async Task<IReadOnlyCollection<ServiceOrder>> GetAllWithDetailsAsync(CancellationToken cancellationToken)
        => await context.ServiceOrders
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<ServiceOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => await context.ServiceOrders
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ServiceOrder?> GetByNumberAndCustomerDocumentAsync(string number, string normalizedDocument, CancellationToken cancellationToken)
        => await context.ServiceOrders
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .FirstOrDefaultAsync(
                x => x.Number == number && x.Customer != null && x.Customer.Document == normalizedDocument,
                cancellationToken);

    public async Task<IReadOnlyCollection<ServiceOrder>> GetAllAsync(CancellationToken cancellationToken)
        => await context.ServiceOrders.ToListAsync(cancellationToken);

    public void Add(ServiceOrder order) => context.ServiceOrders.Add(order);
}
