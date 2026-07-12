using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.Vehicles;

public sealed record GetAllVehiclesQuery : IRequest<IReadOnlyCollection<VehicleResponse>>;
public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleResponse?>;
public sealed record CreateVehicleCommand(VehicleRequest Request) : IRequest<VehicleResponse>;
public sealed record UpdateVehicleCommand(Guid Id, VehicleRequest Request) : IRequest<VehicleResponse?>;
public sealed record DeleteVehicleCommand(Guid Id) : IRequest<bool>;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Request.CustomerId).NotEmpty();
        RuleFor(x => x.Request.Plate).Must(DocumentValidator.IsValidPlate).WithMessage("Placa invalida.");
        RuleFor(x => x.Request.Brand).NotEmpty();
        RuleFor(x => x.Request.Model).NotEmpty();
        RuleFor(x => x.Request.Year).InclusiveBetween(1900, 3000);
    }
}

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.CustomerId).NotEmpty();
        RuleFor(x => x.Request.Plate).Must(DocumentValidator.IsValidPlate).WithMessage("Placa invalida.");
        RuleFor(x => x.Request.Brand).NotEmpty();
        RuleFor(x => x.Request.Model).NotEmpty();
        RuleFor(x => x.Request.Year).InclusiveBetween(1900, 3000);
    }
}

public sealed class GetAllVehiclesQueryHandler(IVehicleRepository repository)
    : IRequestHandler<GetAllVehiclesQuery, IReadOnlyCollection<VehicleResponse>>
{
    public async Task<IReadOnlyCollection<VehicleResponse>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
        => [.. (await repository.GetAllWithCustomerAsync(cancellationToken)).Select(VehicleMapper.ToResponse)];
}

public sealed class GetVehicleByIdQueryHandler(IVehicleRepository repository)
    : IRequestHandler<GetVehicleByIdQuery, VehicleResponse?>
{
    public async Task<VehicleResponse?> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        => (await repository.GetByIdWithCustomerAsync(request.Id, cancellationToken)) is { } vehicle
            ? VehicleMapper.ToResponse(vehicle)
            : null;
}

public sealed class CreateVehicleCommandHandler(IVehicleRepository repository, ICustomerRepository customerRepository, IUnitOfWork context)
    : IRequestHandler<CreateVehicleCommand, VehicleResponse>
{
    public async Task<VehicleResponse> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Cliente nao encontrado para o veiculo informado.");
        }

        var normalizedPlate = DocumentValidator.Normalize(request.Request.Plate);
        if (await repository.ExistsByPlateAsync(normalizedPlate, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Ja existe veiculo cadastrado com esta placa.");
        }

        var vehicle = new Vehicle(request.Request.CustomerId, normalizedPlate, request.Request.Brand, request.Request.Model, request.Request.Year);
        repository.Add(vehicle);
        await context.SaveChangesAsync(cancellationToken);
        return VehicleMapper.ToResponse(vehicle, customer);
    }
}

public sealed class UpdateVehicleCommandHandler(IVehicleRepository repository, IUnitOfWork context)
    : IRequestHandler<UpdateVehicleCommand, VehicleResponse?>
{
    public async Task<VehicleResponse?> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var normalizedPlate = DocumentValidator.Normalize(request.Request.Plate);
        if (await repository.ExistsByPlateExcludingIdAsync(request.Id, normalizedPlate, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Ja existe veiculo cadastrado com esta placa.");
        }

        vehicle.Update(normalizedPlate, request.Request.Brand, request.Request.Model, request.Request.Year);
        await context.SaveChangesAsync(cancellationToken);
        return VehicleMapper.ToResponse(vehicle);
    }
}

public sealed class DeleteVehicleCommandHandler(IVehicleRepository repository, IUnitOfWork context)
    : IRequestHandler<DeleteVehicleCommand, bool>
{
    public async Task<bool> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
        {
            return false;
        }

        repository.Remove(vehicle);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

internal static class VehicleMapper
{
    public static VehicleResponse ToResponse(Vehicle vehicle)
        => ToResponse(vehicle, vehicle.Customer);

    public static VehicleResponse ToResponse(Vehicle vehicle, Customer? customer)
        => new(
            vehicle.Id,
            vehicle.CustomerId,
            vehicle.Plate,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            customer is null
                ? null
                : new VehicleCustomerResponse(
                    customer.Id,
                    customer.Name,
                    customer.Document,
                    customer.Phone,
                    customer.Email));
}

