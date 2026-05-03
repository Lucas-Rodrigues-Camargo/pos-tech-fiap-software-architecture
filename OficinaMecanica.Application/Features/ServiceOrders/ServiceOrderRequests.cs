using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.ServiceOrders;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Application.Features.ServiceOrders;

public sealed record GetAllServiceOrdersQuery : IRequest<IReadOnlyCollection<ServiceOrderResponse>>;
public sealed record GetServiceOrderByIdQuery(Guid Id) : IRequest<ServiceOrderResponse?>;
public sealed record CreateServiceOrderCommand(CreateServiceOrderRequest Request) : IRequest<ServiceOrderResponse>;
public sealed record StartServiceOrderDiagnosisCommand(Guid Id) : IRequest<ServiceOrderResponse?>;
public sealed record UpdateServiceOrderBudgetCommand(Guid Id, UpdateServiceOrderBudgetRequest Request) : IRequest<ServiceOrderResponse?>;
public sealed record AddServiceOrderAdditionalBudgetItemsCommand(Guid Id, UpdateServiceOrderBudgetRequest Request) : IRequest<ServiceOrderBudgetApprovalResponse?>;
public sealed record DecideServiceOrderApprovalCommand(Guid Id, ApprovalDecisionRequest Request) : IRequest<ServiceOrderResponse?>;
public sealed record CancelServiceOrderCommand(Guid Id) : IRequest<ServiceOrderResponse?>;
public sealed record ChangeServiceOrderStatusCommand(Guid Id, ServiceOrderStatus Status) : IRequest<ServiceOrderResponse?>;
public sealed record GetServiceOrderMetricsQuery : IRequest<ExecutionMetricsResponse>;
public sealed record GetServiceOrderTimelineMetricsQuery(Guid Id) : IRequest<ServiceOrderTimelineMetricsResponse?>;
public sealed record GetPublicServiceOrderQuery(string Number, string Document) : IRequest<ServiceOrderResponse?>;

public sealed class CreateServiceOrderCommandValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderCommandValidator()
    {
        RuleFor(x => x.Request.CustomerDocument)
            .Must(DocumentValidator.IsValidCpfOrCnpj)
            .WithMessage("CPF/CNPJ inválido.");

        RuleFor(x => x.Request.Vehicle.Plate)
            .Must(DocumentValidator.IsValidPlate)
            .WithMessage("Placa inválida.");

        RuleFor(x => x.Request.CustomerRequest)
            .NotEmpty()
            .Must(items => items.All(item => !string.IsNullOrWhiteSpace(item)))
            .WithMessage("A lista de solicitações do cliente não pode conter itens vazios.");
    }
}

public sealed class UpdateServiceOrderBudgetCommandValidator : AbstractValidator<UpdateServiceOrderBudgetCommand>
{
    public UpdateServiceOrderBudgetCommandValidator()
    {
        Include(new BudgetItemsCommandValidator<UpdateServiceOrderBudgetCommand>());
    }
}

public sealed class AddServiceOrderAdditionalBudgetItemsCommandValidator : AbstractValidator<AddServiceOrderAdditionalBudgetItemsCommand>
{
    public AddServiceOrderAdditionalBudgetItemsCommandValidator()
    {
        Include(new BudgetItemsCommandValidator<AddServiceOrderAdditionalBudgetItemsCommand>());
    }
}

internal sealed class BudgetItemsCommandValidator<TCommand> : AbstractValidator<TCommand>
{
    public BudgetItemsCommandValidator()
    {
        RuleFor(x => GetId(x)).NotEmpty();
        RuleFor(x => GetRequest(x).Parts).NotNull();
        RuleFor(x => GetRequest(x))
            .Must(request => request.ServiceIds.Count > 0 || request.Parts.Count > 0)
            .WithMessage("Informe ao menos um serviço ou uma peça.");
        RuleForEach(x => GetRequest(x).Parts).ChildRules(part =>
        {
            part.RuleFor(p => p.PartId).NotEmpty();
            part.RuleFor(p => p.Quantity).GreaterThan(0);
        });
    }

    private static Guid GetId(TCommand command)
        => command switch
        {
            UpdateServiceOrderBudgetCommand budget => budget.Id,
            AddServiceOrderAdditionalBudgetItemsCommand additional => additional.Id,
            _ => Guid.Empty
        };

    private static UpdateServiceOrderBudgetRequest GetRequest(TCommand command)
        => command switch
        {
            UpdateServiceOrderBudgetCommand budget => budget.Request,
            AddServiceOrderAdditionalBudgetItemsCommand additional => additional.Request,
            _ => new UpdateServiceOrderBudgetRequest([], [])
        };
}

public sealed class StartServiceOrderDiagnosisCommandValidator : AbstractValidator<StartServiceOrderDiagnosisCommand>
{
    public StartServiceOrderDiagnosisCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class ChangeServiceOrderStatusCommandValidator : AbstractValidator<ChangeServiceOrderStatusCommand>
{
    public ChangeServiceOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => status is ServiceOrderStatus.Finished
                or ServiceOrderStatus.Delivered)
            .WithMessage("Status inválido para transição.");
    }
}

public sealed class DecideServiceOrderApprovalCommandValidator : AbstractValidator<DecideServiceOrderApprovalCommand>
{
    public DecideServiceOrderApprovalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.ApprovalId).NotEmpty();
    }
}

public sealed class CancelServiceOrderCommandValidator : AbstractValidator<CancelServiceOrderCommand>
{
    public CancelServiceOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class GetPublicServiceOrderQueryValidator : AbstractValidator<GetPublicServiceOrderQuery>
{
    public GetPublicServiceOrderQueryValidator()
    {
        RuleFor(x => x.Number).NotEmpty();
        RuleFor(x => x.Document).Must(DocumentValidator.IsValidCpfOrCnpj).WithMessage("CPF/CNPJ inválido.");
    }
}

public sealed class GetAllServiceOrdersQueryHandler(IServiceOrderRepository repository)
    : IRequestHandler<GetAllServiceOrdersQuery, IReadOnlyCollection<ServiceOrderResponse>>
{
    public async Task<IReadOnlyCollection<ServiceOrderResponse>> Handle(GetAllServiceOrdersQuery request, CancellationToken cancellationToken)
        => (await repository.GetAllWithDetailsAsync(cancellationToken)).Select(ServiceOrderMapper.Map).ToList();
}

public sealed class GetServiceOrderByIdQueryHandler(IServiceOrderRepository repository)
    : IRequestHandler<GetServiceOrderByIdQuery, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(GetServiceOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        return order is null ? null : ServiceOrderMapper.Map(order);
    }
}

public sealed class GetPublicServiceOrderQueryHandler(IServiceOrderRepository repository)
    : IRequestHandler<GetPublicServiceOrderQuery, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(GetPublicServiceOrderQuery request, CancellationToken cancellationToken)
    {
        var normalizedDocument = DocumentValidator.Normalize(request.Document);
        var order = await repository.GetByNumberAndCustomerDocumentAsync(request.Number, normalizedDocument, cancellationToken);
        return order is null ? null : ServiceOrderMapper.Map(order);
    }
}

public sealed class CreateServiceOrderCommandHandler(
    ICustomerRepository customerRepository,
    IVehicleRepository vehicleRepository,
    IServiceOrderRepository serviceOrderRepository,
    IOficinaDbContext context)
    : IRequestHandler<CreateServiceOrderCommand, ServiceOrderResponse>
{
    public async Task<ServiceOrderResponse> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var document = DocumentValidator.Normalize(request.Request.CustomerDocument);
        var plate = DocumentValidator.Normalize(request.Request.Vehicle.Plate);

        var customer = await customerRepository.GetByDocumentAsync(document, cancellationToken);
        if (customer is null)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Cliente não encontrado.");
        }

        var vehicle = await vehicleRepository.GetByCustomerAndPlateAsync(customer.Id, plate, cancellationToken);
        if (vehicle is null)
        {
            vehicle = new Vehicle(customer.Id, plate, request.Request.Vehicle.Brand, request.Request.Vehicle.Model, request.Request.Vehicle.Year);
            vehicleRepository.Add(vehicle);
        }

        var order = new ServiceOrder(customer.Id, vehicle.Id, request.Request.CustomerRequest);

        serviceOrderRepository.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        var loaded = await serviceOrderRepository.GetByIdWithDetailsAsync(order.Id, cancellationToken)
            ?? throw new InvalidOperationException("Falha ao carregar a OS recém-criada.");

        return ServiceOrderMapper.Map(loaded);
    }
}

public sealed class UpdateServiceOrderBudgetCommandHandler(
    IServiceCatalogRepository serviceCatalogRepository,
    IPartRepository partRepository,
    IServiceOrderRepository serviceOrderRepository,
    IOficinaDbContext context)
    : IRequestHandler<UpdateServiceOrderBudgetCommand, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(UpdateServiceOrderBudgetCommand request, CancellationToken cancellationToken)
    {
        var order = await serviceOrderRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var services = await serviceCatalogRepository.GetByIdsAsync(request.Request.ServiceIds, cancellationToken);
        if (services.Count != request.Request.ServiceIds.Count)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Um ou mais serviços não foram encontrados.");
        }

        var partIds = request.Request.Parts.Select(x => x.PartId).Distinct().ToList();
        var parts = await partRepository.GetByIdsAsync(partIds, cancellationToken);
        if (parts.Count != partIds.Count)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Uma ou mais peças não foram encontradas.");
        }

        foreach (var service in services)
        {
            order.AddService(service);
        }

        foreach (var requestedPart in request.Request.Parts)
        {
            var part = parts.Single(x => x.Id == requestedPart.PartId);
            part.Reserve(requestedPart.Quantity);
            order.AddPart(part, requestedPart.Quantity);
        }

        order.SendBudget();

        await context.SaveChangesAsync(cancellationToken);
        return ServiceOrderMapper.Map(order);
    }
}

public sealed class AddServiceOrderAdditionalBudgetItemsCommandHandler(
    IServiceCatalogRepository serviceCatalogRepository,
    IPartRepository partRepository,
    IServiceOrderRepository serviceOrderRepository,
    IOficinaDbContext context)
    : IRequestHandler<AddServiceOrderAdditionalBudgetItemsCommand, ServiceOrderBudgetApprovalResponse?>
{
    public async Task<ServiceOrderBudgetApprovalResponse?> Handle(AddServiceOrderAdditionalBudgetItemsCommand request, CancellationToken cancellationToken)
    {
        var order = await serviceOrderRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var approvalId = Guid.NewGuid();

        var services = await serviceCatalogRepository.GetByIdsAsync(request.Request.ServiceIds, cancellationToken);
        if (services.Count != request.Request.ServiceIds.Count)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Um ou mais serviços não foram encontrados.");
        }

        var partIds = request.Request.Parts.Select(x => x.PartId).Distinct().ToList();
        var parts = await partRepository.GetByIdsAsync(partIds, cancellationToken);
        if (parts.Count != partIds.Count)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Uma ou mais peças não foram encontradas.");
        }

        foreach (var service in services)
        {
            order.AddAdditionalService(service, approvalId);
        }

        foreach (var requestedPart in request.Request.Parts)
        {
            var part = parts.Single(x => x.Id == requestedPart.PartId);
            part.Reserve(requestedPart.Quantity);
            order.AddAdditionalPart(part, requestedPart.Quantity, approvalId);
        }

        order.SendAdditionalBudget();

        await context.SaveChangesAsync(cancellationToken);
        return ServiceOrderMapper.MapApproval(order, approvalId);
    }
}

public sealed class StartServiceOrderDiagnosisCommandHandler(IServiceOrderRepository repository, IOficinaDbContext context)
    : IRequestHandler<StartServiceOrderDiagnosisCommand, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(StartServiceOrderDiagnosisCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.StartDiagnosis();
        await context.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.Map(order);
    }
}

public sealed class ChangeServiceOrderStatusCommandHandler(IServiceOrderRepository repository, IOficinaDbContext context)
    : IRequestHandler<ChangeServiceOrderStatusCommand, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(ChangeServiceOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        switch (request.Status)
        {
            case ServiceOrderStatus.Finished:
                order.Finish();
                break;
            case ServiceOrderStatus.Delivered:
                order.Deliver();
                break;
        }

        await context.SaveChangesAsync(cancellationToken);
        return ServiceOrderMapper.Map(order);
    }
}

public sealed class DecideServiceOrderApprovalCommandHandler(
    IPartRepository partRepository,
    IServiceOrderRepository serviceOrderRepository,
    IOficinaDbContext context)
    : IRequestHandler<DecideServiceOrderApprovalCommand, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(DecideServiceOrderApprovalCommand request, CancellationToken cancellationToken)
    {
        var order = await serviceOrderRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (request.Request.Approved)
        {
            if (HasAdditionalApproval(order, request.Request.ApprovalId))
            {
                order.ApproveAdditionalBudget();
            }
            else
            {
                order.ApproveBudget();
            }

            await context.SaveChangesAsync(cancellationToken);
            return ServiceOrderMapper.Map(order);
        }

        var isAdditionalApproval = HasAdditionalApproval(order, request.Request.ApprovalId);
        var partsToRestock = isAdditionalApproval
            ? order.Parts.Where(x => x.AdditionalRequestId == request.Request.ApprovalId).ToList()
            : order.Parts.ToList();
        var partIds = partsToRestock.Select(x => x.PartId).Distinct().ToList();
        if (partIds.Count > 0)
        {
            var parts = await partRepository.GetByIdsAsync(partIds, cancellationToken);
            foreach (var orderPart in partsToRestock)
            {
                var part = parts.SingleOrDefault(x => x.Id == orderPart.PartId);
                part?.Restock(orderPart.Quantity);
            }
        }

        if (isAdditionalApproval)
        {
            order.ReproveAdditionalBudget();
        }
        else
        {
            order.ReproveBudget();
        }

        await context.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.Map(order);
    }

    private static bool HasAdditionalApproval(ServiceOrder order, Guid approvalId)
        => order.Services.Any(x => x.AdditionalRequestId == approvalId)
            || order.Parts.Any(x => x.AdditionalRequestId == approvalId);
}

public sealed class CancelServiceOrderCommandHandler(
    IPartRepository partRepository,
    IServiceOrderRepository serviceOrderRepository,
    IOficinaDbContext context)
    : IRequestHandler<CancelServiceOrderCommand, ServiceOrderResponse?>
{
    public async Task<ServiceOrderResponse?> Handle(CancelServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await serviceOrderRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (order.Status is ServiceOrderStatus.WaitingApproval)
        {
            var partIds = order.Parts.Select(x => x.PartId).Distinct().ToList();
            if (partIds.Count > 0)
            {
                var parts = await partRepository.GetByIdsAsync(partIds, cancellationToken);
                foreach (var orderPart in order.Parts)
                {
                    var part = parts.SingleOrDefault(x => x.Id == orderPart.PartId);
                    part?.Restock(orderPart.Quantity);
                }
            }
        }

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.Map(order);
    }
}

public sealed class GetServiceOrderMetricsQueryHandler(IServiceOrderRepository repository)
    : IRequestHandler<GetServiceOrderMetricsQuery, ExecutionMetricsResponse>
{
    public async Task<ExecutionMetricsResponse> Handle(GetServiceOrderMetricsQuery request, CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        var executionHours = orders
            .Select(x => x.GetExecutionTime())
            .Where(x => x.HasValue)
            .Select(x => (decimal)x!.Value.TotalHours)
            .ToList();

        var average = executionHours.Count == 0 ? 0 : executionHours.Average();
        return new ExecutionMetricsResponse(executionHours.Count, Math.Round(average, 2));
    }
}

public sealed class GetServiceOrderTimelineMetricsQueryHandler(IServiceOrderRepository repository)
    : IRequestHandler<GetServiceOrderTimelineMetricsQuery, ServiceOrderTimelineMetricsResponse?>
{
    public async Task<ServiceOrderTimelineMetricsResponse?> Handle(GetServiceOrderTimelineMetricsQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        return order is null ? null : ServiceOrderMetricsMapper.MapTimeline(order);
    }
}

internal static class ServiceOrderMapper
{
    public static ServiceOrderResponse Map(ServiceOrder order)
        => new(
            order.Id,
            order.Number,
            order.CustomerRequest,
            order.Customer?.Name ?? string.Empty,
            order.Customer?.Document ?? string.Empty,
            order.Vehicle?.Plate ?? string.Empty,
            order.Status,
            order.TotalAmount,
            order.CreatedAtUtc,
            order.DiagnosticStartedAtUtc,
            order.BudgetSentAtUtc,
            order.ApprovedAtUtc,
            order.FinishedAtUtc,
            order.Services.Select(MapService).ToList(),
            order.Parts.Select(MapPart).ToList());

    public static ServiceOrderBudgetApprovalResponse MapApproval(ServiceOrder order, Guid approvalId)
        => new(
            approvalId,
            Map(order),
            MapBudgetItems(
                order.Services.Where(x => x.AdditionalRequestId != approvalId),
                order.Parts.Where(x => x.AdditionalRequestId != approvalId)),
            MapBudgetItems(
                order.Services.Where(x => x.AdditionalRequestId == approvalId),
                order.Parts.Where(x => x.AdditionalRequestId == approvalId)));

    private static ServiceOrderBudgetItemsResponse MapBudgetItems(
        IEnumerable<ServiceOrderServiceItem> services,
        IEnumerable<ServiceOrderPartItem> parts)
    {
        var serviceItems = services.Select(MapService).ToList();
        var partItems = parts.Select(MapPart).ToList();
        var totalAmount = serviceItems.Sum(x => x.UnitPrice) + partItems.Sum(x => x.Total);

        return new ServiceOrderBudgetItemsResponse(serviceItems, partItems, totalAmount);
    }

    private static ServiceOrderLineResponse MapService(ServiceOrderServiceItem item)
        => new(item.ServiceCatalogItemId, item.Description, item.UnitPrice, item.EstimatedHours, item.IsAdditional, item.AdditionalRequestId);

    private static ServiceOrderPartResponse MapPart(ServiceOrderPartItem item)
        => new(item.PartId, item.Description, item.UnitPrice, item.Quantity, item.Total, item.IsAdditional, item.AdditionalRequestId);
}

internal static class ServiceOrderMetricsMapper
{
    public static ServiceOrderTimelineMetricsResponse MapTimeline(ServiceOrder order)
    {
        var finishedAtUtc = order.DeliveredAtUtc
            ?? order.FinishedAtUtc
            ?? order.ApprovedAtUtc
            ?? order.BudgetSentAtUtc
            ?? order.DiagnosticStartedAtUtc
            ?? DateTime.UtcNow;

        return new ServiceOrderTimelineMetricsResponse(
            order.Id,
            order.Number,
            ToHours(finishedAtUtc - order.CreatedAtUtc),
            ToMinutes(finishedAtUtc - order.CreatedAtUtc),
            [
                CreateStep("Recebimento ate diagnostico", order.CreatedAtUtc, order.DiagnosticStartedAtUtc),
                CreateStep("Diagnostico ate envio do orcamento", order.DiagnosticStartedAtUtc, order.BudgetSentAtUtc),
                CreateStep("Envio do orcamento ate aprovacao", order.BudgetSentAtUtc, order.ApprovedAtUtc),
                CreateStep("Aprovacao ate finalizacao", order.ApprovedAtUtc, order.FinishedAtUtc),
                CreateStep("Finalizacao ate entrega", order.FinishedAtUtc, order.DeliveredAtUtc)
            ]);
    }

    private static ServiceOrderStepMetricResponse CreateStep(string step, DateTime? startedAtUtc, DateTime? finishedAtUtc)
    {
        var elapsed = startedAtUtc is not null && finishedAtUtc is not null
            ? finishedAtUtc.Value - startedAtUtc.Value
            : (TimeSpan?)null;

        return new ServiceOrderStepMetricResponse(
            step,
            startedAtUtc,
            finishedAtUtc,
            elapsed is null ? null : ToHours(elapsed.Value),
            elapsed is null ? null : ToMinutes(elapsed.Value));
    }

    private static decimal ToHours(TimeSpan value) => Math.Round((decimal)value.TotalHours, 2);

    private static int ToMinutes(TimeSpan value) => (int)Math.Round(value.TotalMinutes, MidpointRounding.AwayFromZero);
}
