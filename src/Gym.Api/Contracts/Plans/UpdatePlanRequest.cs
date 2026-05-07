namespace Gym.Api.Contracts.Plans;

public sealed record UpdatePlanRequest(
    string Title,
    string? Description,
    decimal Cost,
    int DurationInDays);
