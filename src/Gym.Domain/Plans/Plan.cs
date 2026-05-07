using Gym.Domain.Common;
using Gym.Domain.Common.Result;

namespace Gym.Domain.Plans;

public sealed class Plan : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Cost { get; private set; }
    public int DurationInDays  {  get; private set; }
    public bool IsActive { get; private set; } = true;

    private Plan()
    {
    }

    private Plan(string title, string? description, decimal cost , int durationInDays)
    {
        Title = title;
        Description = description;
        Cost = cost;
        DurationInDays  = durationInDays;
    }

    public static Result<Plan> Create(string title, string? description, decimal cost, int durationInDays)
    {
        var error = Validate(title, cost, durationInDays);
        if (error is not null)
        {
            return error;
        }

        return new Plan(title, description, cost, durationInDays);
    }

    public Result<Updated> UpdateInfo(string title, string? description, decimal cost, int durationInDays )
    {
        if (!IsActive)
        {
            return PlanError.CannotUpdateInactivePlan;
        }

        var error = Validate(title, cost, durationInDays);
        if (error is not null)
        {
            return error;
        }

        Title = title;
        Description = description;
        Cost = cost;
        DurationInDays = durationInDays;

        return Result.Updated;
    }

    public Result<Updated> Activate()
    {
        if (IsActive)
        {
            return PlanError.PlanAlreadyActive;
        }

        IsActive = true;
        return Result.Updated;
    }

    public Result<Updated> Deactivate()
    {
        if (!IsActive)
        {
            return PlanError.PlanAlreadyInactive;
        }

        IsActive = false;
        return Result.Updated;
    }

    private static Error? Validate(string title, decimal cost ,int durationInDays)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return PlanError.TitleRequired;
        }

        if (cost < 0)
        {
            return PlanError.InvalidCost;
        }

        if (durationInDays < 0)
        {
            return PlanError.InvaildDuration;
        }

        return null;
    }
}
