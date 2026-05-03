using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Application.Contracts.ServiceOrders;

public sealed record ServiceOrderResponse(
    Guid Id,
    string Number,
    IReadOnlyCollection<string> CustomerRequest,
    string CustomerName,
    string CustomerDocument,
    string VehiclePlate,
    ServiceOrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime? DiagnosticStartedAtUtc,
    DateTime? BudgetSentAtUtc,
    DateTime? ApprovedAtUtc,
    DateTime? FinishedAtUtc,
    IReadOnlyCollection<ServiceOrderLineResponse> Services,
    IReadOnlyCollection<ServiceOrderPartResponse> Parts);

public sealed record ServiceOrderBudgetApprovalResponse(
    Guid ApprovalId,
    ServiceOrderResponse ServiceOrder,
    ServiceOrderBudgetItemsResponse ExistingItems,
    ServiceOrderBudgetItemsResponse NewAdditionalItems);

public sealed record ServiceOrderBudgetItemsResponse(
    IReadOnlyCollection<ServiceOrderLineResponse> Services,
    IReadOnlyCollection<ServiceOrderPartResponse> Parts,
    decimal TotalAmount);

public sealed record ServiceOrderLineResponse(Guid ServiceId, string Description, decimal UnitPrice, decimal EstimatedHours, bool IsAdditional = false, Guid? AdditionalRequestId = null);
public sealed record ServiceOrderPartResponse(Guid PartId, string Description, decimal UnitPrice, int Quantity, decimal Total, bool IsAdditional = false, Guid? AdditionalRequestId = null);
