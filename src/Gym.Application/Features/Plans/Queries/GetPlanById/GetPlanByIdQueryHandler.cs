using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Plans.Dtos;
using Gym.Application.Features.Plans.Mappers;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Plans.Queries.GetPlanById;

public sealed class GetPlanByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetPlanByIdQueryHandler> logger) : IRequestHandler<GetPlanByIdQuery, Result<PlanDetailsResponse>>
{
    public async Task<Result<PlanDetailsResponse>> Handle(GetPlanByIdQuery query, CancellationToken ct)
    {
        var plan = await context.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == query.Id, ct);
        if (plan is null)
        {
            logger.LogWarning("Plan with ID {PlanId} not found.", query.Id);
            return ApplicationErrors.PlanNotFound;
        }

        return plan.ToDetailsDto();
    }
}
