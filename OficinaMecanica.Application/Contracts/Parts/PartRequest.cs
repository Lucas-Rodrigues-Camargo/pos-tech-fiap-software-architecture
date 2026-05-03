namespace OficinaMecanica.Application.Contracts.Parts;

public sealed record PartRequest(string Name, string Sku, decimal UnitPrice, int StockQuantity);
