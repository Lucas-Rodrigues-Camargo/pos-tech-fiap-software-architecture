namespace OficinaMecanica.Domain.Enums;

public enum ServiceOrderStatus
{
    Received = 1,
    InDiagnosis = 2,
    WaitingApproval = 3,
    InExecution = 4,
    Finished = 5,
    Delivered = 6,
    Reproved = 7,
    Canceled = 8
}
