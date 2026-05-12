using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Customers;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Application.Contracts.Services;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Application.Features.Customers;
using OficinaMecanica.Application.Features.Parts;
using OficinaMecanica.Application.Features.ServicesCatalog;
using OficinaMecanica.Application.Features.Vehicles;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Persistence.Repositories;

namespace OficinaMecanica.Tests.Integration;

public sealed class CrudHandlersIntegrationTests
{
    [Fact]
    public async Task DeveCriarAtualizarListarEBloquearDuplicidadeDeCliente()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new CustomerRepository(database.Context);
        var create = new CreateCustomerCommandHandler(repository, database.Context);
        var update = new UpdateCustomerCommandHandler(repository, database.Context);

        var customer = await create.Handle(
            new CreateCustomerCommand(new CustomerRequest("Maria Silva", "529.982.247-25", "11999999999", "maria@oficina.local")),
            CancellationToken.None);

        var duplicate = await Assert.ThrowsAsync<ValidationException>(() => create.Handle(
            new CreateCustomerCommand(new CustomerRequest("Outra Maria", "52998224725", "11888888888", "outra@oficina.local")),
            CancellationToken.None));

        var updated = await update.Handle(
            new UpdateCustomerCommand(customer.Id, new CustomerRequest("Maria Souza", "52998224725", "11777777777", "maria.souza@oficina.local")),
            CancellationToken.None);

        var listed = await new GetAllCustomersQueryHandler(repository).Handle(new GetAllCustomersQuery(), CancellationToken.None);
        var byId = await new GetCustomerByIdQueryHandler(repository).Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);
        var deleted = await new DeleteCustomerCommandHandler(repository, database.Context).Handle(new DeleteCustomerCommand(customer.Id), CancellationToken.None);
        var missingDelete = await new DeleteCustomerCommandHandler(repository, database.Context).Handle(new DeleteCustomerCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Contains("JÃ¡ existe cliente", duplicate.Message);
        Assert.NotNull(updated);
        Assert.Equal("Maria Souza", updated!.Name);
        Assert.Single(listed);
        Assert.NotNull(byId);
        Assert.True(deleted);
        Assert.False(missingDelete);
    }

    [Fact]
    public async Task DeveCriarAtualizarListarERemoverPeca()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new PartRepository(database.Context);

        var part = await new CreatePartCommandHandler(repository, database.Context).Handle(
            new CreatePartCommand(new PartRequest("Filtro de ar", "flt-002", 42m, 10)),
            CancellationToken.None);

        var duplicateCreate = await Assert.ThrowsAsync<ValidationException>(() => new CreatePartCommandHandler(repository, database.Context).Handle(
            new CreatePartCommand(new PartRequest("Filtro duplicado", "FLT-002", 45m, 3)),
            CancellationToken.None));

        var otherPart = await new CreatePartCommandHandler(repository, database.Context).Handle(
            new CreatePartCommand(new PartRequest("Pastilha de freio", "brk-001", 90m, 4)),
            CancellationToken.None);

        var updatedWithoutSku = await new UpdatePartCommandHandler(repository, database.Context).Handle(
            new UpdatePartCommand(part.Id, new PartRequest("Filtro de ar esportivo", null, 50m, 8)),
            CancellationToken.None);

        var updatedWithSameSku = await new UpdatePartCommandHandler(repository, database.Context).Handle(
            new UpdatePartCommand(part.Id, new PartRequest("Filtro de ar esportivo", "flt-002", 50m, 8)),
            CancellationToken.None);

        var duplicateUpdate = await Assert.ThrowsAsync<ValidationException>(() => new UpdatePartCommandHandler(repository, database.Context).Handle(
            new UpdatePartCommand(part.Id, new PartRequest("Filtro de ar esportivo", otherPart.Sku, 50m, 8)),
            CancellationToken.None));

        var updated = await new UpdatePartCommandHandler(repository, database.Context).Handle(
            new UpdatePartCommand(part.Id, new PartRequest("Filtro de ar esportivo", "flt-003", 50m, 8)),
            CancellationToken.None);

        var all = await new GetAllPartsQueryHandler(repository).Handle(new GetAllPartsQuery(), CancellationToken.None);
        var byId = await new GetPartByIdQueryHandler(repository).Handle(new GetPartByIdQuery(part.Id), CancellationToken.None);
        var removed = await new DeletePartCommandHandler(repository, database.Context).Handle(new DeletePartCommand(part.Id), CancellationToken.None);
        var missingUpdate = await new UpdatePartCommandHandler(repository, database.Context).Handle(
            new UpdatePartCommand(Guid.NewGuid(), new PartRequest("Nao existe", "X", 1m, 1)),
            CancellationToken.None);

        Assert.Contains("SKU", duplicateCreate.Message);
        Assert.Contains("SKU", duplicateUpdate.Message);
        Assert.NotNull(updatedWithoutSku);
        Assert.Equal("FLT-002", updatedWithoutSku!.Sku);
        Assert.NotNull(updatedWithSameSku);
        Assert.Equal("FLT-002", updatedWithSameSku!.Sku);
        Assert.NotNull(updated);
        Assert.Equal("FLT-003", updated!.Sku);
        Assert.Equal(2, all.Count);
        Assert.NotNull(byId);
        Assert.True(removed);
        Assert.Null(missingUpdate);
    }

    [Fact]
    public async Task DeveCriarAtualizarListarERemoverServico()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new ServiceCatalogRepository(database.Context);

        var service = await new CreateServiceCommandHandler(repository, database.Context).Handle(
            new CreateServiceCommand(new ServiceCatalogRequest("Alinhamento", "Alinhamento completo", 120m, 1m)),
            CancellationToken.None);

        var updated = await new UpdateServiceCommandHandler(repository, database.Context).Handle(
            new UpdateServiceCommand(service.Id, new ServiceCatalogRequest("Alinhamento 3D", "Alinhamento computadorizado", 180m, 1.5m)),
            CancellationToken.None);

        var all = await new GetAllServicesQueryHandler(repository).Handle(new GetAllServicesQuery(), CancellationToken.None);
        var byId = await new GetServiceByIdQueryHandler(repository).Handle(new GetServiceByIdQuery(service.Id), CancellationToken.None);
        var removed = await new DeleteServiceCommandHandler(repository, database.Context).Handle(new DeleteServiceCommand(service.Id), CancellationToken.None);
        var missingDelete = await new DeleteServiceCommandHandler(repository, database.Context).Handle(new DeleteServiceCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal("Alinhamento 3D", updated!.Name);
        Assert.Single(all);
        Assert.NotNull(byId);
        Assert.True(removed);
        Assert.False(missingDelete);
    }

    [Fact]
    public async Task DeveCriarAtualizarListarERemoverVeiculo()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        database.Context.Customers.Add(customer);
        await database.Context.SaveChangesAsync();

        var customerRepository = new CustomerRepository(database.Context);
        var vehicleRepository = new VehicleRepository(database.Context);

        var vehicle = await new CreateVehicleCommandHandler(vehicleRepository, customerRepository, database.Context).Handle(
            new CreateVehicleCommand(new VehicleRequest(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020)),
            CancellationToken.None);

        var duplicate = await Assert.ThrowsAsync<ValidationException>(() => new CreateVehicleCommandHandler(vehicleRepository, customerRepository, database.Context).Handle(
            new CreateVehicleCommand(new VehicleRequest(customer.Id, "BRA2E19", "Fiat", "Uno", 2018)),
            CancellationToken.None));

        var updated = await new UpdateVehicleCommandHandler(vehicleRepository, database.Context).Handle(
            new UpdateVehicleCommand(vehicle.Id, new VehicleRequest(customer.Id, "BRA2E20", "Volkswagen", "Polo", 2022)),
            CancellationToken.None);

        var all = await new GetAllVehiclesQueryHandler(vehicleRepository).Handle(new GetAllVehiclesQuery(), CancellationToken.None);
        var byId = await new GetVehicleByIdQueryHandler(vehicleRepository).Handle(new GetVehicleByIdQuery(vehicle.Id), CancellationToken.None);
        var removed = await new DeleteVehicleCommandHandler(vehicleRepository, database.Context).Handle(new DeleteVehicleCommand(vehicle.Id), CancellationToken.None);
        var missingCustomer = await Assert.ThrowsAsync<ValidationException>(() => new CreateVehicleCommandHandler(vehicleRepository, customerRepository, database.Context).Handle(
            new CreateVehicleCommand(new VehicleRequest(Guid.NewGuid(), "BRA2E21", "Ford", "Ka", 2019)),
            CancellationToken.None));

        Assert.Contains("placa", duplicate.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cliente", missingCustomer.Message);
        Assert.Equal(customer.Id, vehicle.CustomerId);
        Assert.Equal(customer.Name, vehicle.Customer?.Name);
        Assert.NotNull(updated);
        Assert.Equal("BRA2E20", updated!.Plate);
        Assert.Single(all);
        Assert.NotNull(byId);
        Assert.Equal(customer.Id, byId!.CustomerId);
        Assert.Equal(customer.Name, byId.Customer?.Name);
        Assert.True(removed);
    }
}
