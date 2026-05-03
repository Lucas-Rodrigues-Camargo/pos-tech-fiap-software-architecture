namespace OficinaMecanica.Application.Contracts.Auth;

public sealed record RegisterRequest(string Username, string Email, string Password, bool IsAdmin = false);
