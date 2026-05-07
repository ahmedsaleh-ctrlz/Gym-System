using Gym.Application.Common.Interfaces;
using Gym.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Identity.Policies;

public class SameCoachRequirement : IAuthorizationRequirement;


public class SameCoachHandler(IHttpContextAccessor httpContext,IAppDbContext dbContext) : AuthorizationHandler<SameCoachRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SameCoachRequirement requirement)
    {
        if(context.User.IsInRole(nameof(Role.Admin)))
        {
            context.Succeed(requirement);
            return ;
        }

        if (!context.User.IsInRole(nameof(Role.Coach)))
        {
            context.Fail();
            return ;
        }

        var routeValue = httpContext.HttpContext!.Request.RouteValues["id"]?.ToString();

        if (!int.TryParse(routeValue, out int requestedCoachId))
        {
            context.Fail();
            return ;
        }

        var personId = context.User.FindFirst("person_id")?.Value;

        if (string.IsNullOrWhiteSpace(personId))
        {
            context.Fail();
            return;
        }

        var isSameCoach = await dbContext.Coaches.AnyAsync
            (c => c.Id == requestedCoachId && c.PersonId == Convert.ToInt32(personId));
        if (isSameCoach) 
        {
            context.Succeed(requirement);
        }

        return; 
    }
}