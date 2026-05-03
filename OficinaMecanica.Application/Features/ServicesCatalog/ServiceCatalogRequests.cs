using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Contracts.Services;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Features.ServicesCatalog;

public sealed record GetAllServicesQuery : IRequest<IReadOnlyCollection<ServiceCatalogItem>>;
public sealed record GetServiceByIdQuery(Guid Id) : IRequest<ServiceCatalogItem?>;
public sealed record CreateServiceCommand(ServiceCatalogRequest Request) : IRequest<ServiceCatalogItem>;
public sealed record UpdateServiceCommand(Guid Id, ServiceCatalogRequest Request) : IRequest<ServiceCatalogItem?>;
public sealed record DeleteServiceCommand(Guid Id) : IRequest<bool>;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Description).NotEmpty();
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.EstimatedHours).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Description).NotEmpty();
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.EstimatedHours).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetAllServicesQueryHandler(IServiceCatalogRepository repository) : IRequestHandler<GetAllServicesQuery, IReadOnlyCollection<ServiceCatalogItem>>
{
    public async Task<IReadOnlyCollection<ServiceCatalogItem>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
        => await repository.GetAllAsync(cancellationToken);
}

public sealed class GetServiceByIdQueryHandler(IServiceCatalogRepository repository) : IRequestHandler<GetServiceByIdQuery, ServiceCatalogItem?>
{
    public async Task<ServiceCatalogItem?> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class CreateServiceCommandHandler(IServiceCatalogRepository repository, IOficinaDbContext context) : IRequestHandler<CreateServiceCommand, ServiceCatalogItem>
{
    public async Task<ServiceCatalogItem> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var item = new ServiceCatalogItem(request.Request.Name, request.Request.Description, request.Request.Price, request.Request.EstimatedHours);
        repository.Add(item);
        await context.SaveChangesAsync(cancellationToken);
        return item;
    }
}

public sealed class UpdateServiceCommandHandler(IServiceCatalogRepository repository, IOficinaDbContext context) : IRequestHandler<UpdateServiceCommand, ServiceCatalogItem?>
{
    public async Task<ServiceCatalogItem?> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.Update(request.Request.Name, request.Request.Description, request.Request.Price, request.Request.EstimatedHours);
        await context.SaveChangesAsync(cancellationToken);
        return item;
    }
}

public sealed class DeleteServiceCommandHandler(IServiceCatalogRepository repository, IOficinaDbContext context) : IRequestHandler<DeleteServiceCommand, bool>
{
    public async Task<bool> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            return false;
        }

        repository.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
