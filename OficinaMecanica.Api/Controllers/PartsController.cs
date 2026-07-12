using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Contracts.Parts;
using OficinaMecanica.Application.UseCases.Parts;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/parts")]
public sealed class PartsController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all parts.")]
    [EndpointDescription("Returns registered parts with SKU, unit price, and available stock quantity.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<Part>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetAllPartsQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a part by id.")]
    [EndpointDescription("Returns the part when the provided id exists.")]
    [ProducesResponseType(typeof(Part), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetPartByIdQuery(id), cancellationToken)) is { } part ? Ok(part) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    [EndpointSummary("Create a stock part.")]
    [EndpointDescription("Creates a part with name, SKU, unit price, and initial stock quantity.")]
    [ProducesResponseType(typeof(Part), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] PartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var part = await mediator.Send(new CreatePartCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a stock part.")]
    [EndpointDescription("Updates registration data, price, and quantity for an existing part.")]
    [ProducesResponseType(typeof(Part), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new UpdatePartCommand(id, request), cancellationToken)) is { } part ? Ok(part) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a part.")]
    [EndpointDescription("Deletes the stock part when the provided id exists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(new DeletePartCommand(id), cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

