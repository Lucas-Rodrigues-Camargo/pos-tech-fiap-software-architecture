using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class Vehicle : Entity
{
    private Vehicle()
    {
    }

    public Vehicle(Guid customerId, string plate, string brand, string model, int year)
    {
        CustomerId = customerId;
        Update(plate, brand, model, year);
    }

    public Guid CustomerId { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }

    public Customer? Customer { get; private set; }

    public void Update(string plate, string brand, string model, int year)
    {
        Plate = plate.Trim().ToUpperInvariant();
        Brand = brand.Trim();
        Model = model.Trim();
        Year = year;
    }
}
