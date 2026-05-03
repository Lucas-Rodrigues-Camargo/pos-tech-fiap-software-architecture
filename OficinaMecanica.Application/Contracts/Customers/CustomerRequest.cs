namespace OficinaMecanica.Application.Contracts.Customers;

public sealed record CustomerRequest(string Name, string Document, string Phone, string Email);
