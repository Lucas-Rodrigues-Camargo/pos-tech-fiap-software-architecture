namespace OficinaMecanica.Application.Contracts.Vehicles;

public sealed record VehicleRequest(Guid CustomerId, string Plate, string Brand, string Model, int Year);
