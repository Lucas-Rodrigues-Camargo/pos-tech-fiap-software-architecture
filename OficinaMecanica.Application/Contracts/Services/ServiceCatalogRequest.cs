namespace OficinaMecanica.Application.Contracts.Services;

public sealed record ServiceCatalogRequest(string Name, string Description, decimal Price, decimal EstimatedHours);
