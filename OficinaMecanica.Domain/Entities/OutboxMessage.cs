using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Entities;

public sealed class OutboxMessage : Entity
{
    private OutboxMessage() { }

    public OutboxMessage(string type, string payload, string correlationId)
    {
        Type = type;
        Payload = payload;
        CorrelationId = correlationId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length <= 1000 ? error : error[..1000];
    }
}
