namespace Gym.Application.Features.Payments.Dtos;

public sealed record PaymentResponse
{
    public int PaymentId { get; set; }
    public int SubscriptionId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAtUtc { get; set; }
}
