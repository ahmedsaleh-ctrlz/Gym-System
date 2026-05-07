using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Plans;

namespace Gym.Application.Features.Plans.Mappers;

public static class PlanMapper
{
    public static PlanDetailsResponse ToDetailsDto(this Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new PlanDetailsResponse
        {
            PlanId = plan.Id,
            Title = plan.Title,
            Description = plan.Description,
            Cost = plan.Cost,
            DurationInDays = plan.DurationInDays,
            IsActive = plan.IsActive
        };
    }
}
