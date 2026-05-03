using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Domain.Entities;

public sealed class ServiceOrder : Entity
{
    private readonly List<ServiceOrderServiceItem> _services = [];
    private readonly List<ServiceOrderPartItem> _parts = [];

    private ServiceOrder()
    {
    }

    public ServiceOrder(Guid customerId, Guid vehicleId, IEnumerable<string> customerRequest)
    {
        CustomerId = customerId;
        VehicleId = vehicleId;
        CustomerRequest = customerRequest
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
        Status = ServiceOrderStatus.Received;
        CreatedAtUtc = DateTime.UtcNow;
        Number = $"OS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    public string Number { get; private set; } = string.Empty;
    public List<string> CustomerRequest { get; private set; } = [];
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DiagnosticStartedAtUtc { get; private set; }
    public DateTime? BudgetSentAtUtc { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public DateTime? ExecutionStartedAtUtc { get; private set; }
    public DateTime? FinishedAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }

    public Customer? Customer { get; private set; }
    public Vehicle? Vehicle { get; private set; }

    public IReadOnlyCollection<ServiceOrderServiceItem> Services => _services;
    public IReadOnlyCollection<ServiceOrderPartItem> Parts => _parts;

    public decimal TotalAmount => _services.Sum(item => item.UnitPrice) + _parts.Sum(item => item.Total);

    public void AddService(ServiceCatalogItem service)
    {
        _services.Add(new ServiceOrderServiceItem(service.Id, service.Name, service.Price, service.EstimatedHours));
    }

    public void AddAdditionalService(ServiceCatalogItem service, Guid additionalRequestId)
    {
        _services.Add(new ServiceOrderServiceItem(service.Id, service.Name, service.Price, service.EstimatedHours, true, additionalRequestId));
    }

    public void AddPart(Part part, int quantity)
    {
        _parts.Add(new ServiceOrderPartItem(part.Id, part.Name, part.UnitPrice, quantity));
    }

    public void AddAdditionalPart(Part part, int quantity, Guid additionalRequestId)
    {
        _parts.Add(new ServiceOrderPartItem(part.Id, part.Name, part.UnitPrice, quantity, true, additionalRequestId));
    }

    public void StartDiagnosis()
    {
        EnsureStatus(ServiceOrderStatus.Received);
        DiagnosticStartedAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.InDiagnosis;
    }

    public void SendBudget()
    {
        if (Status is not ServiceOrderStatus.Received and not ServiceOrderStatus.InDiagnosis)
        {
            throw new InvalidOperationException("O orçamento só pode ser enviado após o recebimento ou diagnóstico.");
        }

        BudgetSentAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.WaitingApproval;
    }

    public void SendAdditionalBudget()
    {
        if (Status is not ServiceOrderStatus.InExecution and not ServiceOrderStatus.WaitingApproval)
        {
            throw new InvalidOperationException("Itens adicionais so podem ser enviados para OS em execucao ou aguardando aprovacao.");
        }

        BudgetSentAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.WaitingApproval;
    }

    public void ApproveBudget()
    {
        EnsureStatus(ServiceOrderStatus.WaitingApproval);
        ApprovedAtUtc = DateTime.UtcNow;
        ExecutionStartedAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.InExecution;
    }

    public void ApproveAdditionalBudget()
    {
        EnsureStatus(ServiceOrderStatus.WaitingApproval);
        Status = ServiceOrderStatus.InExecution;
    }

    public void ReproveBudget()
    {
        EnsureStatus(ServiceOrderStatus.WaitingApproval);
        Status = ServiceOrderStatus.Reproved;
    }

    public void ReproveAdditionalBudget()
    {
        EnsureStatus(ServiceOrderStatus.WaitingApproval);
        Status = ServiceOrderStatus.InExecution;
    }

    public void Cancel()
    {
        if (Status is ServiceOrderStatus.InExecution
            or ServiceOrderStatus.Finished
            or ServiceOrderStatus.Delivered
            or ServiceOrderStatus.Reproved)
        {
            throw new InvalidOperationException($"A OS está em {Status} e não pode ser cancelada.");
        }

        Status = ServiceOrderStatus.Canceled;
    }

    public void Finish()
    {
        EnsureStatus(ServiceOrderStatus.InExecution);
        FinishedAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.Finished;
    }

    public void Deliver()
    {
        EnsureStatus(ServiceOrderStatus.Finished);
        DeliveredAtUtc = DateTime.UtcNow;
        Status = ServiceOrderStatus.Delivered;
    }

    public TimeSpan? GetExecutionTime()
    {
        if (ExecutionStartedAtUtc is null || FinishedAtUtc is null)
        {
            return null;
        }

        return FinishedAtUtc.Value - ExecutionStartedAtUtc.Value;
    }

    private void EnsureStatus(ServiceOrderStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"A OS está em {Status} e não permite esta transição.");
        }
    }
}
