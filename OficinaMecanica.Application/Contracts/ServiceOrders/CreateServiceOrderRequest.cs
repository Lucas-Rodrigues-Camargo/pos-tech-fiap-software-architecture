namespace OficinaMecanica.Application.Contracts.ServiceOrders;

public sealed record CreateServiceOrderRequest(
    string CustomerDocument,
    VehicleSnapshot Vehicle,
    List<string> CustomerRequest);

public sealed record UpdateServiceOrderBudgetRequest(
    IReadOnlyCollection<Guid> ServiceIds,
    IReadOnlyCollection<RequestedPart> Parts);

public sealed record ApprovalDecisionRequest(Guid ApprovalId, bool Approved);

public sealed record VehicleSnapshot(string Plate, string Brand, string Model, int Year);
public sealed record RequestedPart(Guid PartId, int Quantity);
