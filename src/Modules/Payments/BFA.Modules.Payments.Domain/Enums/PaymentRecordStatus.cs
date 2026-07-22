namespace BFA.Modules.Payments.Domain.Enums;

public enum PaymentRecordStatus
{
    Pending = 0,
    Captured = 1,
    Failed = 2,
    Refunded = 3
}
