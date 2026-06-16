using System.Reflection;

using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Plans;

namespace Gym.Tests.Common.Plans;

public static class PlanFactory
{
    public static Result<Plan> CreatePlan(
        int? id = 1,
        string? title = null,
        string? description = "Standard plan",
        decimal? cost = 500m,
        int? durationInDays = 30,
        int? allowedFreezeCount = 2,
        int? maxTotalFreezeDays = 14)
    {
        var result = Plan.Create(
            title ?? "Gold",
            description,
            cost ?? 500m,
            durationInDays ?? 30,
            allowedFreezeCount ?? 2,
            maxTotalFreezeDays ?? 14);

        if (result.IsSuccess)
        {
            SetEntityId(result.Value, id ?? 1);
        }

        return result;
    }

    private static void SetEntityId(Entity entity, int id)
    {
        typeof(Entity)
            .GetProperty(nameof(Entity.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
    }
}