using OficinaMecanica.Domain.Entities;
using OficinaMecanica.Domain.Enums;

namespace OficinaMecanica.Tests.Domain;

public sealed class ServiceOrderTests
{
    [Fact]
    public void DevePercorrerFluxoCompletoDaOs()
    {
        var order = new ServiceOrder(Guid.NewGuid(), Guid.NewGuid(), ["Cliente relata barulho no motor."]);

        Assert.Null(order.DiagnosticStartedAtUtc);

        order.StartDiagnosis();
        order.SendBudget();
        order.ApproveBudget();
        order.Finish();
        order.Deliver();

        Assert.Equal(ServiceOrderStatus.Delivered, order.Status);
        Assert.NotNull(order.DiagnosticStartedAtUtc);
        Assert.NotNull(order.GetExecutionTime());
    }

    [Fact]
    public void DeveFalharAoEntregarAntesDeFinalizar()
    {
        var order = new ServiceOrder(Guid.NewGuid(), Guid.NewGuid(), ["Cliente relata barulho no motor."]);

        var exception = Assert.Throws<InvalidOperationException>(() => order.Deliver());

        Assert.Contains("não permite", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DeveFalharAoCancelarEmExecucao()
    {
        var order = new ServiceOrder(Guid.NewGuid(), Guid.NewGuid(), ["Cliente relata barulho no motor."]);

        order.StartDiagnosis();
        order.SendBudget();
        order.ApproveBudget();

        var exception = Assert.Throws<InvalidOperationException>(() => order.Cancel());

        Assert.Contains("não pode ser cancelada", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}

