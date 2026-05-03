using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Application.Contracts.ServiceOrders;

public sealed record StatusTransitionRequest(ServiceOrderStatus Status);
