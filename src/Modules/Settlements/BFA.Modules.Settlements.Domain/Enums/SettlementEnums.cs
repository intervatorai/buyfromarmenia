namespace BFA.Modules.Settlements.Domain.Enums;

public enum SettlementStatus
{
    Pending = 0,
    Eligible = 1,
    Paid = 2
}

public enum PayoutStatus
{
    Scheduled = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
