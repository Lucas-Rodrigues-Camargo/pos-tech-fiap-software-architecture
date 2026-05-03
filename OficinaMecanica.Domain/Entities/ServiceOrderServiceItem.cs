using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class ServiceOrderServiceItem : Entity
{
    private ServiceOrderServiceItem()
    {
    }

    public ServiceOrderServiceItem(Guid serviceCatalogItemId, string description, decimal unitPrice, decimal estimatedHours, bool isAdditional = false, Guid? additionalRequestId = null)
    {
        ServiceCatalogItemId = serviceCatalogItemId;
        Description = description;
        UnitPrice = unitPrice;
        EstimatedHours = estimatedHours;
        IsAdditional = isAdditional;
        AdditionalRequestId = additionalRequestId;
    }

    public Guid ServiceOrderId { get; private set; }
    public Guid ServiceCatalogItemId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal EstimatedHours { get; private set; }
    public bool IsAdditional { get; private set; }
    public Guid? AdditionalRequestId { get; private set; }
}
