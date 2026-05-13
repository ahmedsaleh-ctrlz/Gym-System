using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;
using Gym.Domain.Plans;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler(
    ILogger<Result<Created>> logger ,
    IAppDbContext context,
    HybridCache cache) 
    : IRequestHandler<CreatePlanCommand, Result<Created>>
{
    public async Task<Result<Created>> Handle(CreatePlanCommand request, CancellationToken ct)
    {
        logger.LogTrace("Handling New Plan Creation");
        var planResult = Plan.Create(request.title,request.description,request.cost,request.durationInDays,request.allowedFreezeCount,request.maxTotalFreezeDays);

        if (planResult.IsError)
        {
            logger.LogError("cannot create plan ,Due to :{Errors}", planResult.TopError);
            return planResult.Errors;
        }

        logger.LogInformation("{Plan Title} Plan Created succfully", planResult.Value.Title);

        await context.Plans.AddAsync(planResult.Value,ct);
        await cache.RemoveByTagAsync("Plan", ct);
        await context.SaveChangesAsync(ct);

        return Result.Created;
    }

    
}
