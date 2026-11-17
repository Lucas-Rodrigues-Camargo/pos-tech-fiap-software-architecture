using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Contracts.Customers;
using OficinaMecanica.Application.UseCases.Customers;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Authorize(Policy = "Employee")]
[Route("api/customers")]
public sealed class CustomersController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List all customers.")]
    [EndpointDescription("Returns all customers available to link vehicles and service orders.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<Customer>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await mediator.Send(new GetAllCustomersQuery(), cancellationToken));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a customer by id.")]
    [EndpointDescription("Returns customer details when the provided id exists.")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new GetCustomerByIdQuery(id), cancellationToken)) is { } customer ? Ok(customer) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost]
    [EndpointSummary("Create a customer.")]
    [EndpointDescription("Creates a customer using a valid CPF or CNPJ and prevents duplicate documents.")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await mediator.Send(new CreateCustomerCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a customer.")]
    [EndpointDescription("Updates the registration data for the customer identified by id.")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await mediator.Send(new UpdateCustomerCommand(id, request), cancellationToken)) is { } customer ? Ok(customer) : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a customer.")]
    [EndpointDescription("Deletes the customer when the provided id exists.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Send(new DeleteCustomerCommand(id), cancellationToken) ? NoContent() : NotFound();
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

