using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OficinaMecanica.Api.Auth;
using OficinaMecanica.Api.Middlewares;
using OficinaMecanica.Application.Behaviors;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Customers;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Application.Contracts.ServiceOrders;
using OficinaMecanica.Application.Contracts.Services;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Application.Features.Customers;
using OficinaMecanica.Application.Features.Parts;
using OficinaMecanica.Application.Features.ServiceOrders;
using OficinaMecanica.Application.Features.ServicesCatalog;
using OficinaMecanica.Application.Features.Vehicles;
using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Infrastructure.Persistence.Repositories;

namespace OficinaMecanica.Tests.Application;

public sealed class ValidationAndMetricsTests
{
    [Theory]
    [InlineData("123.456.789-01", "12345678901", true)]
    [InlineData("12.345.678/0001-90", "12345678000190", true)]
    [InlineData("abc", "ABC", false)]
    public void DeveNormalizarEValidarDocumentos(string input, string normalized, bool valid)
    {
        Assert.Equal(normalized, DocumentValidator.Normalize(input));
        Assert.Equal(valid, DocumentValidator.IsValidCpfOrCnpj(input));
    }

    [Theory]
    [InlineData("BRA2E19", true)]
    [InlineData("ABC1234", true)]
    [InlineData("AB12345", false)]
    public void DeveValidarPlaca(string plate, bool valid)
        => Assert.Equal(valid, DocumentValidator.IsValidPlate(plate));

    [Fact]
    public async Task ValidationBehaviorDeveExecutarProximoQuandoValido()
    {
        var behavior = new ValidationBehavior<CreatePartCommand, Part>(
            [new CreatePartCommandValidator()]);

        var executed = false;
        var result = await behavior.Handle(
            new CreatePartCommand(new PartRequest("Filtro", "FLT", 10m, 5)),
            _ =>
            {
                executed = true;
                return Task.FromResult(new Part("Filtro", "FLT", 10m, 5));
            },
            CancellationToken.None);

        Assert.True(executed);
        Assert.Equal("FLT", result.Sku);
    }

    [Fact]
    public async Task ValidationBehaviorDeveFalharQuandoInvalido()
    {
        var behavior = new ValidationBehavior<CreatePartCommand, Part>(
            [new CreatePartCommandValidator()]);

        await Assert.ThrowsAsync<OficinaMecanica.Application.Common.ValidationException>(() => behavior.Handle(
            new CreatePartCommand(new PartRequest("", "", -1m, -1)),
            _ => Task.FromResult(new Part("Filtro", "FLT", 10m, 5)),
            CancellationToken.None));
    }

    [Fact]
    public void ValidadoresDevemDetectarRequestsInvalidos()
    {
        Assert.False(new CreateCustomerCommandValidator()
            .Validate(new CreateCustomerCommand(new CustomerRequest("", "abc", "", "email-invalido"))).IsValid);
        Assert.False(new UpdateVehicleCommandValidator()
            .Validate(new UpdateVehicleCommand(Guid.Empty, new VehicleRequest(Guid.Empty, "xxx", "", "", 1800))).IsValid);
        Assert.False(new CreateServiceCommandValidator()
            .Validate(new CreateServiceCommand(new ServiceCatalogRequest("", "", -1m, -1m))).IsValid);
        Assert.False(new UpdatePartCommandValidator()
            .Validate(new UpdatePartCommand(Guid.Empty, new PartRequest("", "", -1m, -1))).IsValid);
        Assert.False(new CreateServiceOrderCommandValidator()
            .Validate(new CreateServiceOrderCommand(new CreateServiceOrderRequest("abc", new VehicleSnapshot("xxx", "", "", 1800), [""]))).IsValid);
        Assert.False(new UpdateCustomerCommandValidator()
            .Validate(new UpdateCustomerCommand(Guid.Empty, new CustomerRequest("", "abc", "", "email-invalido"))).IsValid);
        Assert.False(new CreateVehicleCommandValidator()
            .Validate(new CreateVehicleCommand(new VehicleRequest(Guid.Empty, "xxx", "", "", 1800))).IsValid);
        Assert.False(new UpdateServiceCommandValidator()
            .Validate(new UpdateServiceCommand(Guid.Empty, new ServiceCatalogRequest("", "", -1m, -1m))).IsValid);
        Assert.False(new UpdateServiceOrderBudgetCommandValidator()
            .Validate(new UpdateServiceOrderBudgetCommand(Guid.Empty, new UpdateServiceOrderBudgetRequest([], [new RequestedPart(Guid.Empty, 0)]))).IsValid);
        Assert.False(new StartServiceOrderDiagnosisCommandValidator()
            .Validate(new StartServiceOrderDiagnosisCommand(Guid.Empty)).IsValid);
        Assert.False(new DecideServiceOrderApprovalCommandValidator()
            .Validate(new DecideServiceOrderApprovalCommand(Guid.Empty, new ApprovalDecisionRequest(Guid.Empty, true))).IsValid);
        Assert.False(new CancelServiceOrderCommandValidator()
            .Validate(new CancelServiceOrderCommand(Guid.Empty)).IsValid);
        Assert.False(new GetPublicServiceOrderQueryValidator()
            .Validate(new GetPublicServiceOrderQuery("", "abc")).IsValid);
        Assert.False(new ChangeServiceOrderStatusCommandValidator()
            .Validate(new ChangeServiceOrderStatusCommand(Guid.NewGuid(), global::OficinaMecanica.Domain.Enums.ServiceOrderStatus.InExecution)).IsValid);
    }

    [Fact]
    public void ValidadoresDevemAceitarRequestsValidos()
    {
        Assert.True(new CreateVehicleCommandValidator()
            .Validate(new CreateVehicleCommand(new VehicleRequest(Guid.NewGuid(), "BRA2E19", "Volkswagen", "Gol", 2020))).IsValid);
        Assert.True(new UpdateCustomerCommandValidator()
            .Validate(new UpdateCustomerCommand(Guid.NewGuid(), new CustomerRequest("Maria", "12345678901", "11999999999", "maria@oficina.local"))).IsValid);
        Assert.True(new UpdateServiceCommandValidator()
            .Validate(new UpdateServiceCommand(Guid.NewGuid(), new ServiceCatalogRequest("Alinhamento", "Completo", 120m, 1m))).IsValid);
        Assert.True(new UpdateServiceOrderBudgetCommandValidator()
            .Validate(new UpdateServiceOrderBudgetCommand(Guid.NewGuid(), new UpdateServiceOrderBudgetRequest([Guid.NewGuid()], [new RequestedPart(Guid.NewGuid(), 1)]))).IsValid);
        Assert.True(new StartServiceOrderDiagnosisCommandValidator()
            .Validate(new StartServiceOrderDiagnosisCommand(Guid.NewGuid())).IsValid);
        Assert.True(new DecideServiceOrderApprovalCommandValidator()
            .Validate(new DecideServiceOrderApprovalCommand(Guid.NewGuid(), new ApprovalDecisionRequest(Guid.NewGuid(), true))).IsValid);
        Assert.True(new CancelServiceOrderCommandValidator()
            .Validate(new CancelServiceOrderCommand(Guid.NewGuid())).IsValid);
        Assert.True(new GetPublicServiceOrderQueryValidator()
            .Validate(new GetPublicServiceOrderQuery("OS-123", "12345678901")).IsValid);
    }

    [Fact]
    public async Task DeveGerarMetricasPorOrdem()
    {
        await using var database = await Integration.TestDatabase.CreateAsync();
        var customer = new Customer("Joao Silva", "12345678901", "11999999999", "joao@oficina.local");
        var vehicle = new Vehicle(customer.Id, "BRA2E19", "Volkswagen", "Gol", 2020);
        var service = new ServiceCatalogItem("Alinhamento", "Alinhamento completo", 120m, 1m);
        var order = new ServiceOrder(customer.Id, vehicle.Id, ["Cliente relata volante torto."]);

        database.Context.Customers.Add(customer);
        database.Context.Vehicles.Add(vehicle);
        database.Context.ServicesCatalog.Add(service);
        database.Context.ServiceOrders.Add(order);
        await database.Context.SaveChangesAsync();

        order.StartDiagnosis();
        order.AddService(service);
        order.SendBudget();
        order.ApproveBudget();
        order.Finish();
        order.Deliver();
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var repository = new ServiceOrderRepository(database.Context);
        var metrics = await new GetServiceOrderTimelineMetricsQueryHandler(repository)
            .Handle(new GetServiceOrderTimelineMetricsQuery(order.Id), CancellationToken.None);
        var aggregate = await new GetServiceOrderMetricsQueryHandler(repository)
            .Handle(new GetServiceOrderMetricsQuery(), CancellationToken.None);

        Assert.NotNull(metrics);
        Assert.Equal(order.Id, metrics!.ServiceOrderId);
        Assert.Equal(5, metrics.Steps.Count);
        Assert.Equal(1, aggregate.FinishedOrders);
    }

    [Fact]
    public void JwtTokenServiceDeveGerarTokenComRole()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-dev-only-2026",
                ["Jwt:Issuer"] = "OficinaMecanica",
                ["Jwt:Audience"] = "OficinaMecanica.Admin"
            })
            .Build();

        var response = new JwtTokenService(configuration).Generate("admin", ["Admin"]);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.True(response.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task MiddlewareDeveConverterInvalidOperationEmBadRequest()
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("falhou"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("falhou", body);
    }

    [Fact]
    public async Task MiddlewareDeveConverterValidationExceptionEPropagarQuandoNaoHaErro()
    {
        var validation = new ExceptionHandlingMiddleware(_ => throw new OficinaMecanica.Application.Common.ValidationException("invalido"));
        var validationContext = new DefaultHttpContext();
        validationContext.Response.Body = new MemoryStream();

        await validation.InvokeAsync(validationContext);

        validationContext.Response.Body.Position = 0;
        var body = await new StreamReader(validationContext.Response.Body).ReadToEndAsync();

        var executed = false;
        var success = new ExceptionHandlingMiddleware(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        var successContext = new DefaultHttpContext();

        await success.InvokeAsync(successContext);

        Assert.Equal(StatusCodes.Status400BadRequest, validationContext.Response.StatusCode);
        Assert.Contains("invalido", body);
        Assert.True(executed);
        Assert.Equal(StatusCodes.Status200OK, successContext.Response.StatusCode);
    }
}
