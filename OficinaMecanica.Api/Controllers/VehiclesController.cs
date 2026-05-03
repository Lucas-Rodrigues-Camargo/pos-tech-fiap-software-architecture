using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Contracts.Vehicles;
using OficinaMecanica.Application.Features.Vehicles;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public sealed class VehiclesController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all vehicles.")]
    [EndpointDescription("Returns vehicles with their linked customer owner.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<Vehicle>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetAllVehiclesQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a vehicle by id.")]
    [EndpointDescription("Returns the vehicle when the provided id exists.")]
    [ProducesResponseType(typeof(Vehicle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetVehicleByIdQuery(id), cancellationToken)) is { } vehicle ? Ok(vehicle) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    [EndpointSummary("Create a vehicle.")]
    [EndpointDescription("Creates a vehicle for an existing customer and validates duplicate plates.")]
    [ProducesResponseType(typeof(Vehicle), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] VehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await mediator.Send(new CreateVehicleCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a vehicle.")]
    [EndpointDescription("Updates plate, brand, model, or year for an existing vehicle.")]
    [ProducesResponseType(typeof(Vehicle), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new UpdateVehicleCommand(id, request), cancellationToken)) is { } vehicle ? Ok(vehicle) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a vehicle.")]
    [EndpointDescription("Deletes the vehicle when the provided id exists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(new DeleteVehicleCommand(id), cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

