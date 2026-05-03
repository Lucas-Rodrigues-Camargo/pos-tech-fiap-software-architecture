using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class ServiceOrderPartItem : Entity
{
    private ServiceOrderPartItem()
    {
    }

    public ServiceOrderPartItem(Guid partId, string description, decimal unitPrice, int quantity, bool isAdditional = false, Guid? additionalRequestId = null)
    {
        PartId = partId;
        Description = description;
        UnitPrice = unitPrice;
        Quantity = quantity;
        IsAdditional = isAdditional;
        AdditionalRequestId = additionalRequestId;
    }

    public Guid ServiceOrderId { get; private set; }
    public Guid PartId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public bool IsAdditional { get; private set; }
    public Guid? AdditionalRequestId { get; private set; }

    public decimal Total => UnitPrice * Quantity;
}
