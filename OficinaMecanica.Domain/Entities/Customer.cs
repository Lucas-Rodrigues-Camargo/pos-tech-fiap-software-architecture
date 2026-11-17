using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class Customer : Entity
{
    private Customer()
    {
    }

    public Customer(string name, string document, string phone, string email)
    {
        Update(name, document, phone, email);
    }

    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();

    public void Update(string name, string document, string phone, string email)
    {
        Name = name.Trim();
        Document = document.Trim();
        Phone = phone.Trim();
        Email = email.Trim();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
