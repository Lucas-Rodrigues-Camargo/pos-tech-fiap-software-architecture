using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Contracts.ServiceOrders;
using OficinaMecanica.Application.UseCases.ServiceOrders;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/service-orders")]
public sealed class ServiceOrdersController(IMediator mediator) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    [EndpointSummary("List all service orders.")]
    [EndpointDescription("Returns service orders with customer, vehicle, services, parts, and current status.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ServiceOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetAllServiceOrdersQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a service order by id.")]
    [EndpointDescription("Returns full service order details when the provided id exists.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetServiceOrderByIdQuery(id), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost("{id:guid}/notifications/email")]
    [EndpointSummary("Send the current service order status by email.")]
    [EndpointDescription("Sends the current status to the email registered for the service order customer using the configured SMTP service.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendStatusEmail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(new SendServiceOrderStatusEmailCommand(id), cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost]
    [EndpointSummary("Create a received service order.")]
    [EndpointDescription("Creates a service order with customer document, vehicle data, and customer request list. The initial status is Received.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(new CreateServiceOrderCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost("{id:guid}/status")]
    [EndpointSummary("Change the service order operational status.")]
    [EndpointDescription("Receives the enum number and only allows finishing or delivering the service order.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] StatusTransitionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new ChangeServiceOrderStatusCommand(id, request.Status), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPatch("{id:guid}/budget")]
    [EndpointSummary("Add a budget to the service order.")]
    [EndpointDescription("Adds services and parts, reserves required stock, and moves the order to WaitingApproval.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateServiceOrderBudgetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new UpdateServiceOrderBudgetCommand(id, request), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPatch("{id:guid}/budget/additional-items")]
    [EndpointSummary("Add additional budget items to the service order.")]
    [EndpointDescription("Adds additional services and parts, reserves required stock, marks the current additional request, and returns only the new items separated for approval.")]
    [ProducesResponseType(typeof(ServiceOrderBudgetApprovalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAdditionalBudgetItems(Guid id, [FromBody] UpdateServiceOrderBudgetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new AddServiceOrderAdditionalBudgetItemsCommand(id, request), cancellationToken)) is { } approval ? Ok(approval) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost("{id:guid}/diagnosis/start")]
    [EndpointSummary("Start service order diagnosis.")]
    [EndpointDescription("Sets diagnosticStartedAtUtc and moves the order to InDiagnosis.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartDiagnosis(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new StartServiceOrderDiagnosisCommand(id), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost("{id:guid}/approval")]
    [EndpointSummary("Approve or reject the service order budget.")]
    [EndpointDescription("When approved, moves to InExecution. When rejected, moves to Reproved and returns reserved parts to stock.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecideApproval(Guid id, [FromBody] ApprovalDecisionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new DecideServiceOrderApprovalCommand(id, request), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpPost("{id:guid}/cancel")]
    [EndpointSummary("Cancel a service order.")]
    [EndpointDescription("Cancels orders in Received, InDiagnosis, or WaitingApproval. It does not allow canceling InExecution, Finished, Delivered, or Reproved.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new CancelServiceOrderCommand(id), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpGet("metrics/execution-time")]
    [EndpointSummary("Get general execution time metrics.")]
    [EndpointDescription("Returns the number of finished orders and the average execution time in hours.")]
    [ProducesResponseType(typeof(ExecutionMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetServiceOrderMetricsQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [Authorize]
    [HttpGet("{id:guid}/metrics/execution-time")]
    [EndpointSummary("Get execution time metrics by service order.")]
    [EndpointDescription("Returns total elapsed time and the elapsed time between each step for the provided service order.")]
    [ProducesResponseType(typeof(ServiceOrderTimelineMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetricsById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetServiceOrderTimelineMetricsQuery(id), cancellationToken)) is { } metrics ? Ok(metrics) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [AllowAnonymous]
    [HttpGet("public/{number}")]
    [EndpointSummary("Get public service order status.")]
    [EndpointDescription("Allows querying a service order by number and customer document without authentication.")]
    [ProducesResponseType(typeof(ServiceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicStatus(string number, [FromQuery] string document, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetPublicServiceOrderQuery(number, document), cancellationToken)) is { } order ? Ok(order) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

