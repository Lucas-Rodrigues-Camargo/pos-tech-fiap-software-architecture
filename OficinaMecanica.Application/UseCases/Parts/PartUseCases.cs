using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.Parts;

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

public sealed class CreatePartCommandHandler(IPartRepository repository, IUnitOfWork context) : IRequestHandler<CreatePartCommand, Part>
{
    public async Task<Part> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        var normalizedSku = PartSku.NormalizeRequired(request.Request.Sku);
        if (await repository.ExistsBySkuAsync(normalizedSku, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Ja existe peca cadastrada com este SKU.");
        }

        var part = new Part(request.Request.Name, normalizedSku, request.Request.UnitPrice, request.Request.StockQuantity);
        repository.Add(part);
        await context.SaveChangesAsync(cancellationToken);
        return part;
    }
}

public sealed class UpdatePartCommandHandler(IPartRepository repository, IUnitOfWork context) : IRequestHandler<UpdatePartCommand, Part?>
{
    public async Task<Part?> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (part is null)
        {
            return null;
        }

        var normalizedSku = PartSku.NormalizeOptional(request.Request.Sku) ?? part.Sku;
        var skuChanged = !string.Equals(part.Sku, normalizedSku, StringComparison.Ordinal);
        if (skuChanged && await repository.ExistsBySkuExcludingIdAsync(request.Id, normalizedSku, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Ja existe peca cadastrada com este SKU.");
        }

        part.Update(request.Request.Name, normalizedSku, request.Request.UnitPrice, request.Request.StockQuantity);
        await context.SaveChangesAsync(cancellationToken);
        return part;
    }
}

public sealed class DeletePartCommandHandler(IPartRepository repository, IUnitOfWork context) : IRequestHandler<DeletePartCommand, bool>
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

internal static class PartSku
{
    public static string NormalizeRequired(string? sku) => sku!.Trim().ToUpperInvariant();

    public static string? NormalizeOptional(string? sku)
        => string.IsNullOrWhiteSpace(sku) ? null : sku.Trim().ToUpperInvariant();
}
