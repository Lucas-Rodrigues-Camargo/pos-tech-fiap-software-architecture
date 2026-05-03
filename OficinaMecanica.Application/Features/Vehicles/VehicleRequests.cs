using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.Features.Vehicles;

public sealed record GetAllVehiclesQuery : IRequest<IReadOnlyCollection<Vehicle>>;
public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<Vehicle?>;
public sealed record CreateVehicleCommand(VehicleRequest Request) : IRequest<Vehicle>;
public sealed record UpdateVehicleCommand(Guid Id, VehicleRequest Request) : IRequest<Vehicle?>;
public sealed record DeleteVehicleCommand(Guid Id) : IRequest<bool>;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Request.CustomerId).NotEmpty();
        RuleFor(x => x.Request.Plate).Must(DocumentValidator.IsValidPlate).WithMessage("Placa inválida.");
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
        RuleFor(x => x.Request.Plate).Must(DocumentValidator.IsValidPlate).WithMessage("Placa inválida.");
        RuleFor(x => x.Request.Brand).NotEmpty();
        RuleFor(x => x.Request.Model).NotEmpty();
        RuleFor(x => x.Request.Year).InclusiveBetween(1900, 3000);
    }
}

public sealed class GetAllVehiclesQueryHandler(IVehicleRepository repository)
    : IRequestHandler<GetAllVehiclesQuery, IReadOnlyCollection<Vehicle>>
{
    public async Task<IReadOnlyCollection<Vehicle>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
        => await repository.GetAllWithCustomerAsync(cancellationToken);
}

public sealed class GetVehicleByIdQueryHandler(IVehicleRepository repository)
    : IRequestHandler<GetVehicleByIdQuery, Vehicle?>
{
    public async Task<Vehicle?> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        => await repository.GetByIdWithCustomerAsync(request.Id, cancellationToken);
}

public sealed class CreateVehicleCommandHandler(IVehicleRepository repository, ICustomerRepository customerRepository, IOficinaDbContext context)
    : IRequestHandler<CreateVehicleCommand, Vehicle>
{
    public async Task<Vehicle> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        if (await customerRepository.GetByIdAsync(request.Request.CustomerId, cancellationToken) is null)
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Cliente não encontrado para o veículo informado.");
        }

        var normalizedPlate = DocumentValidator.Normalize(request.Request.Plate);
        if (await repository.ExistsByPlateAsync(normalizedPlate, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Já existe veículo cadastrado com esta placa.");
        }

        var vehicle = new Vehicle(request.Request.CustomerId, normalizedPlate, request.Request.Brand, request.Request.Model, request.Request.Year);
        repository.Add(vehicle);
        await context.SaveChangesAsync(cancellationToken);
        return vehicle;
    }
}

public sealed class UpdateVehicleCommandHandler(IVehicleRepository repository, IOficinaDbContext context)
    : IRequestHandler<UpdateVehicleCommand, Vehicle?>
{
    public async Task<Vehicle?> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
        {
            return null;
        }

        var normalizedPlate = DocumentValidator.Normalize(request.Request.Plate);
        if (await repository.ExistsByPlateExcludingIdAsync(request.Id, normalizedPlate, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Já existe veículo cadastrado com esta placa.");
        }

        vehicle.Update(normalizedPlate, request.Request.Brand, request.Request.Model, request.Request.Year);
        await context.SaveChangesAsync(cancellationToken);
        return vehicle;
    }
}

public sealed class DeleteVehicleCommandHandler(IVehicleRepository repository, IOficinaDbContext context)
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
