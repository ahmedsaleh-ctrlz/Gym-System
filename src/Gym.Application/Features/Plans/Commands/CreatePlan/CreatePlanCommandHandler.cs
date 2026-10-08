using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Gym.Domain.Notifications.Enums;
using Gym.Domain.Plans;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler(
    ILogger<CreatePlanCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IIdentityService identityService,
    INotificationBackgroundJobs notificationBackgroundJobs)
    : IRequestHandler<CreatePlanCommand, Result<Created>>
{
    public async Task<Result<Created>> Handle(
        CreatePlanCommand request,
        CancellationToken ct)
    {
        logger.LogTrace("Handling new plan creation.");

        var planResult = Plan.Create(
            request.Title,
            request.Description,
            request.Cost,
            request.DurationInDays,
            request.AllowedFreezeCount,
            request.MaxTotalFreezeDays);

        if (planResult.IsError)
        {
            logger.LogError(
                "Cannot create plan. Errors: {Errors}",
                planResult.Errors);

            return planResult.Errors;
        }

        context.Plans.Add(planResult.Value);

        await context.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync("Plan", ct);
        await cache.RemoveByTagAsync("AdminDashboard:PlansSummary", ct);
        await cache.RemoveByTagAsync("AdminDashboard:Overview", ct);

        var userIdsResult = await identityService
            .GetUsersIdsByRoleAsync(
                Role.Member,
                ct);

        if (userIdsResult.IsSuccess && userIdsResult.Value.Any())
        {
            notificationBackgroundJobs.SendToUsers(
                userIdsResult.Value,
                "New Plan Available 🎉",
                $"A new plan '{planResult.Value.Title}' is now available.",
                NotificationType.General);
        }

        logger.LogInformation(
            "{PlanTitle} plan created successfully.",
            planResult.Value.Title);

        return Result.Created;
    }
}
