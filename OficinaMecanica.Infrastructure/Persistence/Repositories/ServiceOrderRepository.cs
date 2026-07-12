using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class ServiceOrderRepository(OficinaDbContext context) : IServiceOrderRepository
{
    public async Task<IReadOnlyCollection<ServiceOrder>> GetAllWithDetailsAsync(CancellationToken cancellationToken)
        => await context.ServiceOrders
            .Include(x => x.Customer)
            .Include(x => x.Vehicle)
            .Include(x => x.Services)
            .Include(x => x.Parts)
            .Where(x => x.Status != ServiceOrderStatus.Finished
                && x.Status != ServiceOrderStatus.Delivered
                && x.Status != ServiceOrderStatus.Reproved
                && x.Status != ServiceOrderStatus.Canceled)
            .OrderBy(x => x.Status == ServiceOrderStatus.InExecution ? 0
                : x.Status == ServiceOrderStatus.WaitingApproval ? 1
                : x.Status == ServiceOrderStatus.InDiagnosis ? 2
                : 3)
            .ThenBy(x => x.CreatedAtUtc)
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
