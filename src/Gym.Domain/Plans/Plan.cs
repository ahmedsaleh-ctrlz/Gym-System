using Gym.Domain.Common;
using Gym.Domain.Common.Result;

namespace Gym.Domain.Plans;

public sealed class Plan : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Cost { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Plan()
    {
    }

    private Plan(string title, string? description, decimal cost)
    {
        Title = title;
        Description = description;
        Cost = cost;
    }

    public static Result<Plan> Create(string title, string? description, decimal cost)
    {
        var error = Validate(title, cost);
        if (error is not null)
        {
            return error;
        }

        return new Plan(title, description, cost);
    }

    public Result<Updated> UpdateInfo(string title, string? description, decimal cost)
    {
        if (!IsActive)
        {
            return PlanError.CannotUpdateInactivePlan;
        }

        var error = Validate(title, cost);
        if (error is not null)
        {
            return error;
        }

        Title = title;
        Description = description;
        Cost = cost;

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

    private static Error? Validate(string title, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return PlanError.TitleRequired;
        }

        if (cost < 0)
        {
            return PlanError.InvalidCost;
        }

        return null;
    }
}
