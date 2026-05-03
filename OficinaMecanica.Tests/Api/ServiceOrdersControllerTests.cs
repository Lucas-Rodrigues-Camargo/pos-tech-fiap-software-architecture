using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Controllers;
using OficinaMecanica.Application.Contracts.ServiceOrders;
using OficinaMecanica.Application.Features.ServiceOrders;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Tests.Api;

public sealed class ServiceOrdersControllerTests
{
    [Fact]
    public async Task DeveCobrirEndpointsDeServiceOrder()
    {
        var id = Guid.NewGuid();
        var response = Response(id, ServiceOrderStatus.Received);
        var metrics = new ServiceOrderTimelineMetricsResponse(id, "OS-1", 1.5m, 90, []);
        var aggregate = new ExecutionMetricsResponse(1, 2.5m);

        var controller = new ServiceOrdersController(new FakeMediator(request => request switch
        {
            GetAllServiceOrdersQuery => new List<ServiceOrderResponse> { response },
            GetServiceOrderByIdQuery => response,
            CreateServiceOrderCommand => response,
            ChangeServiceOrderStatusCommand => Response(id, ServiceOrderStatus.Finished),
            UpdateServiceOrderBudgetCommand => Response(id, ServiceOrderStatus.WaitingApproval),
            AddServiceOrderAdditionalBudgetItemsCommand => Approval(id),
            StartServiceOrderDiagnosisCommand => Response(id, ServiceOrderStatus.InDiagnosis),
            DecideServiceOrderApprovalCommand => Response(id, ServiceOrderStatus.InExecution),
            CancelServiceOrderCommand => Response(id, ServiceOrderStatus.Canceled),
            GetServiceOrderMetricsQuery => aggregate,
            GetServiceOrderTimelineMetricsQuery => metrics,
            GetPublicServiceOrderQuery => response,
            _ => null
        }));

        Assert.IsType<OkObjectResult>(await controller.GetAll(CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetById(id, CancellationToken.None));
        Assert.IsType<CreatedAtActionResult>(await controller.Create(new CreateServiceOrderRequest("12345678901", new VehicleSnapshot("BRA2E19", "VW", "Gol", 2020), ["Barulho"]), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.ChangeStatus(id, new StatusTransitionRequest(ServiceOrderStatus.Finished), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.UpdateBudget(id, new UpdateServiceOrderBudgetRequest([Guid.NewGuid()], []), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.AddAdditionalBudgetItems(id, new UpdateServiceOrderBudgetRequest([Guid.NewGuid()], []), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.StartDiagnosis(id, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.DecideApproval(id, new ApprovalDecisionRequest(Guid.NewGuid(), true), CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.Cancel(id, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetMetrics(CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetMetricsById(id, CancellationToken.None));
        Assert.IsType<OkObjectResult>(await controller.GetPublicStatus("OS-1", "12345678901", CancellationToken.None));
    }

    [Fact]
    public async Task DeveRetornarNotFoundQuandoServiceOrderNaoExiste()
    {
        var controller = new ServiceOrdersController(new FakeMediator(_ => null));
        var id = Guid.NewGuid();

        Assert.IsType<NotFoundResult>(await controller.GetById(id, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.ChangeStatus(id, new StatusTransitionRequest(ServiceOrderStatus.Finished), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.UpdateBudget(id, new UpdateServiceOrderBudgetRequest([], []), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.AddAdditionalBudgetItems(id, new UpdateServiceOrderBudgetRequest([], []), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.StartDiagnosis(id, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.DecideApproval(id, new ApprovalDecisionRequest(Guid.NewGuid(), false), CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.Cancel(id, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.GetMetricsById(id, CancellationToken.None));
        Assert.IsType<NotFoundResult>(await controller.GetPublicStatus("OS-1", "12345678901", CancellationToken.None));
    }

    private static ServiceOrderResponse Response(Guid id, ServiceOrderStatus status)
        => new(
            id,
            "OS-1",
            ["Barulho"],
            "Joao",
            "12345678901",
            "BRA2E19",
            status,
            100m,
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            [],
            []);

    private static ServiceOrderBudgetApprovalResponse Approval(Guid id)
        => new(
            Guid.NewGuid(),
            Response(id, ServiceOrderStatus.WaitingApproval),
            new ServiceOrderBudgetItemsResponse([], [], 0m),
            new ServiceOrderBudgetItemsResponse([], [], 0m));
}
