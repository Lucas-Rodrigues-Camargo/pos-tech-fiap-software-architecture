using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class ServiceCatalogItem : Entity
{
    private ServiceCatalogItem()
    {
    }

    public ServiceCatalogItem(string name, string description, decimal price, decimal estimatedHours)
    {
        Update(name, description, price, estimatedHours);
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal EstimatedHours { get; private set; }

    public void Update(string name, string description, decimal price, decimal estimatedHours)
    {
        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        EstimatedHours = estimatedHours;
    }
}
