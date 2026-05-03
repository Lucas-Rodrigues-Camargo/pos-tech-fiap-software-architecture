using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository(OficinaDbContext context) : IVehicleRepository
{
    public async Task<IReadOnlyCollection<Vehicle>> GetAllWithCustomerAsync(CancellationToken cancellationToken)
        => await context.Vehicles.Include(x => x.Customer).OrderBy(x => x.Plate).ToListAsync(cancellationToken);

    public async Task<Vehicle?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken)
        => await context.Vehicles.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Vehicles.FindAsync([id], cancellationToken);

    public async Task<Vehicle?> GetByCustomerAndPlateAsync(Guid customerId, string plate, CancellationToken cancellationToken)
        => await context.Vehicles.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Plate == plate, cancellationToken);

    public async Task<bool> ExistsByPlateAsync(string plate, CancellationToken cancellationToken)
        => await context.Vehicles.AnyAsync(x => x.Plate == plate, cancellationToken);

    public async Task<bool> ExistsByPlateExcludingIdAsync(Guid id, string plate, CancellationToken cancellationToken)
        => await context.Vehicles.AnyAsync(x => x.Id != id && x.Plate == plate, cancellationToken);

    public void Add(Vehicle vehicle) => context.Vehicles.Add(vehicle);
    public void Remove(Vehicle vehicle) => context.Vehicles.Remove(vehicle);
}
