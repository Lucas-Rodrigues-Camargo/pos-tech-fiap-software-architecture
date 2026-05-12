using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Application.Contracts.ServiceOrders;
using OficinaMecanica.Application.Features.ServiceOrders;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;
using OficinaMecanica.Infrastructure.Persistence;
using OficinaMecanica.Infrastructure.Persistence.Repositories;

namespace OficinaMecanica.Tests.Integration;

public sealed class ServiceOrderIntegrationTests
{
    [Fact]
    public async Task DeveCriarOsEReservarEstoque()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new OficinaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var serviceItem = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var part = new Part("Filtro de oleo", "FLT-001", 35m, 5);

        context.Customers.Add(customer);
        context.Vehicles.Add(vehicle);
        context.ServicesCatalog.Add(serviceItem);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var handler = new CreateServiceOrderCommandHandler(
            new CustomerRepository(context),
            new VehicleRepository(context),
            new ServiceOrderRepository(context),
            context);

        var request = new CreateServiceOrderRequest(
            "529.982.247-25",
            new VehicleSnapshot("BRA2E19", "Volkswagen", "Gol", 2020),
            ["Cliente relata barulho no motor.", "Cliente relata vazamento de oleo."]);

        var response = await handler.Handle(new CreateServiceOrderCommand(request), CancellationToken.None);
        context.ChangeTracker.Clear();

        var budgetHandler = new UpdateServiceOrderBudgetCommandHandler(
            new ServiceCatalogRepository(context),
            new PartRepository(context),
            new ServiceOrderRepository(context),
            context);

        response = await budgetHandler.Handle(
            new UpdateServiceOrderBudgetCommand(
                response.Id,
                new UpdateServiceOrderBudgetRequest(
                    [serviceItem.Id],
                    [new RequestedPart(part.Id, 2)])),
            CancellationToken.None) ?? throw new InvalidOperationException("OS nao encontrada.");

        var updatedPart = await context.Parts.FindAsync(part.Id);

        Assert.Equal(250m, response.TotalAmount);
        Assert.Equal(["Cliente relata barulho no motor.", "Cliente relata vazamento de oleo."], response.CustomerRequest);
        Assert.Equal(ServiceOrderStatus.WaitingApproval, response.Status);
        Assert.NotNull(updatedPart);
        Assert.Equal(3, updatedPart!.StockQuantity);
        Assert.Single(response.Services);
        Assert.Single(response.Parts);
    }

    [Fact]
    public async Task DeveReprovarOrcamentoEDevolverPecasAoEstoque()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new OficinaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var serviceItem = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var part = new Part("Filtro de oleo", "FLT-001", 35m, 5);

        context.Customers.Add(customer);
        context.Vehicles.Add(vehicle);
        context.ServicesCatalog.Add(serviceItem);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var createHandler = new CreateServiceOrderCommandHandler(
            new CustomerRepository(context),
            new VehicleRepository(context),
            new ServiceOrderRepository(context),
            context);

        var response = await createHandler.Handle(
            new CreateServiceOrderCommand(
                new CreateServiceOrderRequest(
                    "529.982.247-25",
                    new VehicleSnapshot("BRA2E19", "Volkswagen", "Gol", 2020),
                    ["Cliente relata barulho no motor."])),
            CancellationToken.None);

        context.ChangeTracker.Clear();

        var budgetHandler = new UpdateServiceOrderBudgetCommandHandler(
            new ServiceCatalogRepository(context),
            new PartRepository(context),
            new ServiceOrderRepository(context),
            context);

        response = await budgetHandler.Handle(
            new UpdateServiceOrderBudgetCommand(
                response.Id,
                new UpdateServiceOrderBudgetRequest(
                    [serviceItem.Id],
                    [new RequestedPart(part.Id, 2)])),
            CancellationToken.None) ?? throw new InvalidOperationException("OS nao encontrada.");

        context.ChangeTracker.Clear();

        var approvalHandler = new DecideServiceOrderApprovalCommandHandler(
            new PartRepository(context),
            new ServiceOrderRepository(context),
            context);

        response = await approvalHandler.Handle(
            new DecideServiceOrderApprovalCommand(
                response.Id,
                new ApprovalDecisionRequest(Guid.NewGuid(), false)),
            CancellationToken.None) ?? throw new InvalidOperationException("OS nao encontrada.");

        var updatedPart = await context.Parts.FindAsync(part.Id);

        Assert.Equal(ServiceOrderStatus.Reproved, response.Status);
        Assert.NotNull(updatedPart);
        Assert.Equal(5, updatedPart!.StockQuantity);
    }

    [Fact]
    public async Task DeveCancelarOrcamentoEDevolverPecasAoEstoque()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new OficinaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var serviceItem = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var part = new Part("Filtro de oleo", "FLT-001", 35m, 5);

        context.Customers.Add(customer);
        context.Vehicles.Add(vehicle);
        context.ServicesCatalog.Add(serviceItem);
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        var createHandler = new CreateServiceOrderCommandHandler(
            new CustomerRepository(context),
            new VehicleRepository(context),
            new ServiceOrderRepository(context),
            context);

        var response = await createHandler.Handle(
            new CreateServiceOrderCommand(
                new CreateServiceOrderRequest(
                    "529.982.247-25",
                    new VehicleSnapshot("BRA2E19", "Volkswagen", "Gol", 2020),
                    ["Cliente relata vazamento de oleo."])),
            CancellationToken.None);

        context.ChangeTracker.Clear();

        var budgetHandler = new UpdateServiceOrderBudgetCommandHandler(
            new ServiceCatalogRepository(context),
            new PartRepository(context),
            new ServiceOrderRepository(context),
            context);

        response = await budgetHandler.Handle(
            new UpdateServiceOrderBudgetCommand(
                response.Id,
                new UpdateServiceOrderBudgetRequest(
                    [serviceItem.Id],
                    [new RequestedPart(part.Id, 2)])),
            CancellationToken.None) ?? throw new InvalidOperationException("OS nao encontrada.");

        context.ChangeTracker.Clear();

        var cancelHandler = new CancelServiceOrderCommandHandler(
            new PartRepository(context),
            new ServiceOrderRepository(context),
            context);

        response = await cancelHandler.Handle(new CancelServiceOrderCommand(response.Id), CancellationToken.None)
            ?? throw new InvalidOperationException("OS nao encontrada.");

        var updatedPart = await context.Parts.FindAsync(part.Id);

        Assert.Equal(ServiceOrderStatus.Canceled, response.Status);
        Assert.NotNull(updatedPart);
        Assert.Equal(5, updatedPart!.StockQuantity);
    }

    [Fact]
    public async Task DeveAdicionarItensAdicionaisESepararUltimaAprovacao()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var initialService = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var additionalService = new ServiceCatalogItem("Alinhamento", "Alinhamento completo", 120m, 1m);
        var initialPart = new Part("Filtro de oleo", "FLT-001", 35m, 5);
        var additionalPart = new Part("Pastilha de freio", "PST-001", 80m, 4);
        var order = new ServiceOrder(customer.Id, vehicle.Id, ["Cliente relata vazamento."]);

        database.Context.Customers.Add(customer);
        database.Context.Vehicles.Add(vehicle);
        database.Context.ServicesCatalog.AddRange(initialService, additionalService);
        database.Context.Parts.AddRange(initialPart, additionalPart);
        database.Context.ServiceOrders.Add(order);
        await database.Context.SaveChangesAsync();

        order.StartDiagnosis();
        order.AddService(initialService);
        initialPart.Reserve(1);
        order.AddPart(initialPart, 1);
        order.SendBudget();
        order.ApproveBudget();
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var approval = await new AddServiceOrderAdditionalBudgetItemsCommandHandler(
                new ServiceCatalogRepository(database.Context),
                new PartRepository(database.Context),
                new ServiceOrderRepository(database.Context),
                database.Context)
            .Handle(
                new AddServiceOrderAdditionalBudgetItemsCommand(
                    order.Id,
                    new UpdateServiceOrderBudgetRequest(
                        [additionalService.Id],
                        [new RequestedPart(additionalPart.Id, 2)])),
                CancellationToken.None) ?? throw new InvalidOperationException("OS nao encontrada.");

        var updatedPart = await database.Context.Parts.FindAsync(additionalPart.Id);

        Assert.Equal(ServiceOrderStatus.WaitingApproval, approval.ServiceOrder.Status);
        Assert.Equal(215m, approval.ExistingItems.TotalAmount);
        Assert.Equal(280m, approval.NewAdditionalItems.TotalAmount);
        Assert.Single(approval.ExistingItems.Services);
        Assert.Single(approval.ExistingItems.Parts);
        Assert.Single(approval.NewAdditionalItems.Services);
        Assert.Single(approval.NewAdditionalItems.Parts);
        Assert.All(approval.NewAdditionalItems.Services, item => Assert.Equal(approval.ApprovalId, item.AdditionalRequestId));
        Assert.All(approval.NewAdditionalItems.Parts, item => Assert.Equal(approval.ApprovalId, item.AdditionalRequestId));
        Assert.NotEqual(approval.ApprovalId, approval.ExistingItems.Services.Single().AdditionalRequestId);
        Assert.NotNull(updatedPart);
        Assert.Equal(2, updatedPart!.StockQuantity);
    }

    [Fact]
    public async Task DeveConsultarOrdemPorIdListaEConsultaPublica()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var order = new ServiceOrder(customer.Id, vehicle.Id, ["Cliente relata barulho."]);

        database.Context.Customers.Add(customer);
        database.Context.Vehicles.Add(vehicle);
        database.Context.ServiceOrders.Add(order);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var repository = new ServiceOrderRepository(database.Context);
        var all = await new GetAllServiceOrdersQueryHandler(repository).Handle(new GetAllServiceOrdersQuery(), CancellationToken.None);
        var byId = await new GetServiceOrderByIdQueryHandler(repository).Handle(new GetServiceOrderByIdQuery(order.Id), CancellationToken.None);
        var publicOrder = await new GetPublicServiceOrderQueryHandler(repository).Handle(
            new GetPublicServiceOrderQuery(order.Number, "529.982.247-25"),
            CancellationToken.None);
        var missingById = await new GetServiceOrderByIdQueryHandler(repository).Handle(new GetServiceOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);
        var missingPublic = await new GetPublicServiceOrderQueryHandler(repository).Handle(
            new GetPublicServiceOrderQuery("OS-000", "52998224725"),
            CancellationToken.None);

        Assert.Single(all);
        Assert.NotNull(byId);
        Assert.NotNull(publicOrder);
        Assert.Equal(order.Number, publicOrder!.Number);
        Assert.Null(missingById);
        Assert.Null(missingPublic);
    }

    [Fact]
    public async Task DeveIniciarDiagnosticoFinalizarEEntregar()
    {
        await using var database = await TestDatabase.CreateAsync();
        var customer = new Customer("Joao Silva", "52998224725", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var serviceItem = new ServiceCatalogItem("Troca de oleo", "Troca de oleo e filtro", 180m, 1.5m);
        var order = new ServiceOrder(customer.Id, vehicle.Id, ["Cliente relata vazamento."]);

        database.Context.Customers.Add(customer);
        database.Context.Vehicles.Add(vehicle);
        database.Context.ServicesCatalog.Add(serviceItem);
        database.Context.ServiceOrders.Add(order);
        await database.Context.SaveChangesAsync();

        order.StartDiagnosis();
        order.AddService(serviceItem);
        order.SendBudget();
        order.ApproveBudget();
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var repository = new ServiceOrderRepository(database.Context);
        var missingDiagnosis = await new StartServiceOrderDiagnosisCommandHandler(repository, database.Context)
            .Handle(new StartServiceOrderDiagnosisCommand(Guid.NewGuid()), CancellationToken.None);
        var finished = await new ChangeServiceOrderStatusCommandHandler(repository, database.Context)
            .Handle(new ChangeServiceOrderStatusCommand(order.Id, ServiceOrderStatus.Finished), CancellationToken.None);
        var delivered = await new ChangeServiceOrderStatusCommandHandler(repository, database.Context)
            .Handle(new ChangeServiceOrderStatusCommand(order.Id, ServiceOrderStatus.Delivered), CancellationToken.None);
        var missingStatus = await new ChangeServiceOrderStatusCommandHandler(repository, database.Context)
            .Handle(new ChangeServiceOrderStatusCommand(Guid.NewGuid(), ServiceOrderStatus.Finished), CancellationToken.None);

        Assert.Null(missingDiagnosis);
        Assert.NotNull(finished);
        Assert.Equal(ServiceOrderStatus.Finished, finished!.Status);
        Assert.NotNull(delivered);
        Assert.Equal(ServiceOrderStatus.Delivered, delivered!.Status);
        Assert.Null(missingStatus);
    }

    [Fact]
    public async Task DeveRetornarNuloQuandoNaoEncontrarBudgetAprovacaoCancelamentoEMetricas()
    {
        await using var database = await TestDatabase.CreateAsync();
        var orderRepository = new ServiceOrderRepository(database.Context);
        var partRepository = new PartRepository(database.Context);
        var serviceRepository = new ServiceCatalogRepository(database.Context);

        var budget = await new UpdateServiceOrderBudgetCommandHandler(serviceRepository, partRepository, orderRepository, database.Context)
            .Handle(new UpdateServiceOrderBudgetCommand(Guid.NewGuid(), new UpdateServiceOrderBudgetRequest([Guid.NewGuid()], [])), CancellationToken.None);
        var approval = await new DecideServiceOrderApprovalCommandHandler(partRepository, orderRepository, database.Context)
            .Handle(new DecideServiceOrderApprovalCommand(Guid.NewGuid(), new ApprovalDecisionRequest(Guid.NewGuid(), true)), CancellationToken.None);
        var cancel = await new CancelServiceOrderCommandHandler(partRepository, orderRepository, database.Context)
            .Handle(new CancelServiceOrderCommand(Guid.NewGuid()), CancellationToken.None);
        var timeline = await new GetServiceOrderTimelineMetricsQueryHandler(orderRepository)
            .Handle(new GetServiceOrderTimelineMetricsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(budget);
        Assert.Null(approval);
        Assert.Null(cancel);
        Assert.Null(timeline);
    }
}
