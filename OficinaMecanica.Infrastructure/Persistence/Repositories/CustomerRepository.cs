using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(OficinaDbContext context) : ICustomerRepository
{
    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Customers.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Customers.FindAsync([id], cancellationToken);

    public async Task<Customer?> GetByDocumentAsync(string document, CancellationToken cancellationToken)
        => await context.Customers.FirstOrDefaultAsync(x => x.Document == document, cancellationToken);

    public async Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken)
        => await context.Customers.AnyAsync(x => x.Document == document, cancellationToken);

    public async Task<bool> ExistsByDocumentExcludingIdAsync(Guid id, string document, CancellationToken cancellationToken)
        => await context.Customers.AnyAsync(x => x.Id != id && x.Document == document, cancellationToken);

    public void Add(Customer customer) => context.Customers.Add(customer);
    public void Remove(Customer customer) => context.Customers.Remove(customer);
}
