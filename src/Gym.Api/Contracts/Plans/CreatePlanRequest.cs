namespace Gym.Api.Contracts.Plans;

public sealed record CreatePlanRequest(
    string Title,
    string? Description,
    decimal Cost,
    int DurationInDays);
