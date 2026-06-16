namespace Gym.Application.Features.Plans.Dtos;

public sealed record PlanDetailsResponse
{
    public int PlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public int DurationInDays { get; set; }
    public bool IsActive { get; set; }
    public int AllowedFreezeCount { get; set; }
    public int MaxTotalFreezeDays { get; set; }
}