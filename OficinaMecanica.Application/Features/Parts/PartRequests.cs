using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Features.Parts;

public sealed record GetAllPartsQuery : IRequest<IReadOnlyCollection<Part>>;
public sealed record GetPartByIdQuery(Guid Id) : IRequest<Part?>;
public sealed record CreatePartCommand(PartRequest Request) : IRequest<Part>;
public sealed record UpdatePartCommand(Guid Id, PartRequest Request) : IRequest<Part?>;
public sealed record DeletePartCommand(Guid Id) : IRequest<bool>;

public sealed class CreatePartCommandValidator : AbstractValidator<CreatePartCommand>
{
    public CreatePartCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Sku).NotEmpty();
        RuleFor(x => x.Request.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
    public UpdatePartCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Sku).NotEmpty();
        RuleFor(x => x.Request.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetAllPartsQueryHandler(IPartRepository repository) : IRequestHandler<GetAllPartsQuery, IReadOnlyCollection<Part>>
{
    public async Task<IReadOnlyCollection<Part>> Handle(GetAllPartsQuery request, CancellationToken cancellationToken)
        => await repository.GetAllAsync(cancellationToken);
}

public sealed class GetPartByIdQueryHandler(IPartRepository repository) : IRequestHandler<GetPartByIdQuery, Part?>
{
    public async Task<Part?> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class CreatePartCommandHandler(IPartRepository repository, IOficinaDbContext context) : IRequestHandler<CreatePartCommand, Part>
{
    public async Task<Part> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var part = new Part(request.Request.Name, request.Request.Sku, request.Request.UnitPrice, request.Request.StockQuantity);
        repository.Add(part);
        await context.SaveChangesAsync(cancellationToken);
        return part;
    }
}

public sealed class UpdatePartCommandHandler(IPartRepository repository, IOficinaDbContext context) : IRequestHandler<UpdatePartCommand, Part?>
{
    public async Task<Part?> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (part is null)
        {
            return null;
        }

        part.Update(request.Request.Name, request.Request.Sku, request.Request.UnitPrice, request.Request.StockQuantity);
        await context.SaveChangesAsync(cancellationToken);
        return part;
    }
}

public sealed class DeletePartCommandHandler(IPartRepository repository, IOficinaDbContext context) : IRequestHandler<DeletePartCommand, bool>
{
    public async Task<bool> Handle(DeletePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (part is null)
        {
            return false;
        }

        repository.Remove(part);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
