using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class Part : Entity
{
    private Part()
    {
    }

    public Part(string name, string sku, decimal unitPrice, int stockQuantity)
    {
        Update(name, sku, unitPrice, stockQuantity);
    }

    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int StockQuantity { get; private set; }

    public void Update(string name, string sku, decimal unitPrice, int stockQuantity)
    {
        Name = name.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        UnitPrice = unitPrice;
        StockQuantity = stockQuantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("A quantidade reservada deve ser maior que zero.");
        }

        if (StockQuantity < quantity)
        {
            throw new InvalidOperationException($"Estoque insuficiente para a peça {Name}.");
        }

        StockQuantity -= quantity;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        StockQuantity += quantity;
    }
}
