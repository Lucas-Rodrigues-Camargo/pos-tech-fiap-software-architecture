using MediatR;

namespace OficinaMecanica.Tests.Api;

internal sealed class FakeMediator(Func<object, object?> handler) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => Task.FromResult((TResponse)handler(request)!);

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        handler(request);
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => Task.FromResult(handler(request));

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        => AsyncEnumerable.Empty<TResponse>();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => AsyncEnumerable.Empty<object?>();

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        handler(notification);
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        handler(notification);
        return Task.CompletedTask;
    }
}
