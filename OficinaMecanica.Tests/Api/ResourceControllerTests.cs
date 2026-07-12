using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Controllers;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Customers;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Application.Contracts.Services;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Application.UseCases.Customers;
using OficinaMecanica.Application.UseCases.Parts;
using OficinaMecanica.Application.UseCases.ServicesCatalog;
using OficinaMecanica.Application.UseCases.Vehicles;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Tests.Api;

public sealed class ResourceControllerTests
{
    [Fact]
    public async Task CustomersControllerDeveRetornarResultadosEsperados()
    {
        var customer = new Customer("Joao", "52998224725", "11999999999", "joao@oficina.local");
        var controller = new CustomersController(new FakeMediator(request => request switch
        {
            GetAllCustomersQuery => new List<Customer> { customer },
            GetCustomerByIdQuery => customer,
            CreateCustomerCommand => customer,
            UpdateCustomerCommand => customer,
            DeleteCustomerCommand => true,
            _ => throw new ValidationException("erro")
        }));

        Assert.IsType<OkObjectResult>(await controller.GetAll(CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetById(customer.Id, CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(await controller.Create(new CustomerRequest("Joao", "52998224725", "11999999999", "joao@oficina.local"), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.Update(customer.Id, new CustomerRequest("Joao", "52998224725", "11999999999", "joao@oficina.local"), CancellationToken.None));
        Assert.IsType<NoContentResult>(await controller.Delete(customer.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PartsControllerDeveTratarNotFoundEBadRequest()
    {
        var part = new Part("Filtro", "FLT", 10m, 5);
        var controller = new PartsController(new FakeMediator(request => request switch
        {
            GetAllPartsQuery => new List<Part> { part },
            GetPartByIdQuery => null,
            CreatePartCommand => part,
            UpdatePartCommand => throw new ValidationException("peca invalida"),
            DeletePartCommand => false,
            _ => null
        }));

        Assert.IsType<OkObjectResult>(await controller.GetAll(CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.GetById(Guid.NewGuid(), CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(await controller.Create(new PartRequest("Filtro", "FLT", 10m, 5), CancellationToken.None));
        Assert.IsType<BadRequestObjectResult>(await controller.Update(part.Id, new PartRequest("", "", -1m, -1), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.Delete(part.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ServicesControllerDeveRetornarCrud()
    {
        var service = new ServiceCatalogItem("Alinhamento", "Alinhamento completo", 120m, 1m);
        var controller = new ServicesController(new FakeMediator(request => request switch
        {
            GetAllServicesQuery => new List<ServiceCatalogItem> { service },
            GetServiceByIdQuery => service,
            CreateServiceCommand => service,
            UpdateServiceCommand => null,
            DeleteServiceCommand => true,
            _ => null
        }));

        Assert.IsType<OkObjectResult>(await controller.GetAll(CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetById(service.Id, CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(await controller.Create(new ServiceCatalogRequest("Alinhamento", "Completo", 120m, 1m), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.Update(service.Id, new ServiceCatalogRequest("Alinhamento", "Completo", 120m, 1m), CancellationToken.None));
        Assert.IsType<NoContentResult>(await controller.Delete(service.Id, CancellationToken.None));
    }

    [Fact]
    public async Task VehiclesControllerDeveRetornarCrud()
    {
        var customerId = Guid.NewGuid();
        var vehicle = new Vehicle(customerId, "BRA2E19", "Volkswagen", "Gol", 2020);
        var vehicleResponse = new VehicleResponse(vehicle.Id, customerId, vehicle.Plate, vehicle.Brand, vehicle.Model, vehicle.Year, null);
        var controller = new VehiclesController(new FakeMediator(request => request switch
        {
            GetAllVehiclesQuery => new List<VehicleResponse> { vehicleResponse },
            GetVehicleByIdQuery => vehicleResponse,
            CreateVehicleCommand => vehicleResponse,
            UpdateVehicleCommand => vehicleResponse,
            DeleteVehicleCommand => false,
            _ => null
        }));

        var request = new VehicleRequest(customerId, "BRA2E19", "Volkswagen", "Gol", 2020);

        Assert.IsType<OkObjectResult>(await controller.GetAll(CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetById(vehicle.Id, CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(await controller.Create(request, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.Update(vehicle.Id, request, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.Delete(vehicle.Id, CancellationToken.None));
    }
}
