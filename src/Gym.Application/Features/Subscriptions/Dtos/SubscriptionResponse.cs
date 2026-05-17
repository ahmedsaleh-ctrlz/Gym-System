using Gym.Domain.Plans;

namespace Gym.Application.Features.Subscriptions.Dtos;

public sealed record SubscriptionResponse
{
    public int SuccriptionId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Status { get; set; }
    public int FreezeCountUsed { get; set; }
    public int TotalFreezeDaysUsed { get;set; }
}
