using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Abstractions.Repositories;

public interface IVehicleRepository
{
    Task<IReadOnlyCollection<Vehicle>> GetAllWithCustomerAsync(CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdWithCustomerAsync(Guid id, CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Vehicle?> GetByCustomerAndPlateAsync(Guid customerId, string plate, CancellationToken cancellationToken);
    Task<bool> ExistsByPlateAsync(string plate, CancellationToken cancellationToken);
    Task<bool> ExistsByPlateExcludingIdAsync(Guid id, string plate, CancellationToken cancellationToken);
    void Add(Vehicle vehicle);
    void Remove(Vehicle vehicle);
}
