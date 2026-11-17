using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Infrastructure.Persistence;

namespace OficinaMecanica.Infrastructure.Notifications;

public sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IAmazonSQS sqs,
    string queueUrl,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        do
        {
            await DispatchBatchAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        var messages = await context.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null && message.Attempts < 10)
            .OrderBy(message => message.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = message.Payload,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        ["eventType"] = new() { DataType = "String", StringValue = message.Type },
                        ["correlationId"] = new() { DataType = "String", StringValue = message.CorrelationId }
                    }
                }, cancellationToken);
                message.MarkProcessed();
                using var payload = JsonDocument.Parse(message.Payload);
                var status = payload.RootElement.GetProperty("status").GetString();
                var durationHours = payload.RootElement.GetProperty("durationHours").GetDouble();
                logger.LogInformation(
                    "Published service order event {EventType} with status {Status} and duration {DurationHours}",
                    message.Type,
                    status,
                    durationHours);
            }
            catch (Exception exception)
            {
                message.MarkFailed(exception.Message);
                logger.LogError(exception, "Failed to publish outbox message {MessageId}", message.Id);
            }
        }

        if (messages.Count > 0) await context.SaveChangesAsync(cancellationToken);
    }
}
