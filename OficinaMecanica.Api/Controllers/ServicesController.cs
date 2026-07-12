using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Contracts.Services;
using OficinaMecanica.Application.UseCases.ServicesCatalog;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/services")]
public sealed class ServicesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List catalog services.")]
    [EndpointDescription("Returns services available to compose a service order budget.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ServiceCatalogItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetAllServicesQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a catalog service by id.")]
    [EndpointDescription("Returns the registered catalog service when the provided id exists.")]
    [ProducesResponseType(typeof(ServiceCatalogItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetServiceByIdQuery(id), cancellationToken)) is { } item ? Ok(item) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    [EndpointSummary("Create a catalog service.")]
    [EndpointDescription("Creates a catalog service with description, price, and estimated hours.")]
    [ProducesResponseType(typeof(ServiceCatalogItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] ServiceCatalogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await mediator.Send(new CreateServiceCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a catalog service.")]
    [EndpointDescription("Updates description, price, or estimated hours for an existing catalog service.")]
    [ProducesResponseType(typeof(ServiceCatalogItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ServiceCatalogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new UpdateServiceCommand(id, request), cancellationToken)) is { } item ? Ok(item) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a catalog service.")]
    [EndpointDescription("Deletes the catalog service when the provided id exists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(new DeleteServiceCommand(id), cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

