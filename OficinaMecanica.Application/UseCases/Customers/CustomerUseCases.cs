using FluentValidation;
using MediatR;
using OficinaMecanica.Application.Abstractions;
using OficinaMecanica.Application.Abstractions.Repositories;
using OficinaMecanica.Application.Common;
using OficinaMecanica.Application.Contracts.Customers;
using OficinaMecanica.Domain.Entities;

namespace OficinaMecanica.Application.UseCases.Customers;

public sealed record GetAllCustomersQuery : IRequest<IReadOnlyCollection<Customer>>;
public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Customer?>;
public sealed record CreateCustomerCommand(CustomerRequest Request) : IRequest<Customer>;
public sealed record UpdateCustomerCommand(Guid Id, CustomerRequest Request) : IRequest<Customer?>;
public sealed record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Phone).NotEmpty();
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Document)
            .Must(DocumentValidator.IsValidCpfOrCnpj)
            .WithMessage("CPF/CNPJ inválido.");
    }
}

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty();
        RuleFor(x => x.Request.Phone).NotEmpty();
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Document)
            .Must(DocumentValidator.IsValidCpfOrCnpj)
            .WithMessage("CPF/CNPJ inválido.");
    }
}

public sealed class GetAllCustomersQueryHandler(ICustomerRepository repository)
    : IRequestHandler<GetAllCustomersQuery, IReadOnlyCollection<Customer>>
{
    public async Task<IReadOnlyCollection<Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        => await repository.GetAllAsync(cancellationToken);
}

public sealed class GetCustomerByIdQueryHandler(ICustomerRepository repository)
    : IRequestHandler<GetCustomerByIdQuery, Customer?>
{
    public async Task<Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(request.Id, cancellationToken);
}

public sealed class CreateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork context)
    : IRequestHandler<CreateCustomerCommand, Customer>
{
    public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var normalizedDocument = DocumentValidator.Normalize(request.Request.Document);

        if (await repository.ExistsByDocumentAsync(normalizedDocument, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Já existe cliente cadastrado com este CPF/CNPJ.");
        }

        var customer = new Customer(request.Request.Name, normalizedDocument, request.Request.Phone, request.Request.Email);
        repository.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
        return customer;
    }
}

public sealed class UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork context)
    : IRequestHandler<UpdateCustomerCommand, Customer?>
{
    public async Task<Customer?> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        var normalizedDocument = DocumentValidator.Normalize(request.Request.Document);
        if (await repository.ExistsByDocumentExcludingIdAsync(request.Id, normalizedDocument, cancellationToken))
        {
            throw new OficinaMecanica.Application.Common.ValidationException("Já existe cliente cadastrado com este CPF/CNPJ.");
        }

        customer.Update(request.Request.Name, normalizedDocument, request.Request.Phone, request.Request.Email);
        await context.SaveChangesAsync(cancellationToken);
        return customer;
    }
}

public sealed class DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork context)
    : IRequestHandler<DeleteCustomerCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
        {
            return false;
        }

        repository.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
