namespace OficinaMecanica.Application.Contracts.ServiceOrders;

public sealed record ExecutionMetricsResponse(int FinishedOrders, decimal AverageExecutionHours);

public sealed record ServiceOrderTimelineMetricsResponse(
    Guid ServiceOrderId,
    string Number,
    decimal TotalElapsedHours,
    int TotalElapsedMinutes,
    IReadOnlyCollection<ServiceOrderStepMetricResponse> Steps);

public sealed record ServiceOrderStepMetricResponse(
    string Step,
    DateTime? StartedAtUtc,
    DateTime? FinishedAtUtc,
    decimal? ElapsedHours,
    int? ElapsedMinutes);
