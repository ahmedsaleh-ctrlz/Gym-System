namespace Gym.Domain.Payments.Enums;

public enum PaymentStatus
{
    Unknown = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}
