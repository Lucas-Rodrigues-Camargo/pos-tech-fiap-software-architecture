namespace OficinaMecanica.Application.Contracts.Vehicles;

public sealed record VehicleResponse(
    Guid Id,
    Guid CustomerId,
    string Plate,
    string Brand,
    string Model,
    int Year,
    VehicleCustomerResponse? Customer);

public sealed record VehicleCustomerResponse(
    Guid Id,
    string Name,
    string Document,
    string Phone,
    string Email);
