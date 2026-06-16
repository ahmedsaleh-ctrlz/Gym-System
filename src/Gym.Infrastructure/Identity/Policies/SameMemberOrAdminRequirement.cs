using Gym.Application.Common.Interfaces;
using Gym.Domain.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Identity.Policies;

public sealed record SameMemberOrAdminRequirement : IAuthorizationRequirement;

public sealed class SameMemberOrAdminRequirementHandler(IAppDbContext dbContext, IHttpContextAccessor httpContext) : AuthorizationHandler<SameMemberOrAdminRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SameMemberOrAdminRequirement requirement)
    {
        // Admin Bypass
        if (context.User.IsInRole(nameof(Role.Admin)))
        {
            context.Succeed(requirement);
            return;
        }

        if (!context.User.IsInRole(nameof(Role.Member)))
        {
            context.Fail();
            return;
        }

        var routeValue = httpContext.HttpContext!.Request.RouteValues["id"]?.ToString();

        if (!int.TryParse(routeValue, out int requestedmemberId))
        {
            context.Fail();
            return;
        }

        var personId = context.User.FindFirst("person_id")?.Value;

        if (string.IsNullOrWhiteSpace(personId))
        {
            context.Fail();
            return;
        }

        var isSameCoach = await dbContext.Members.AnyAsync(
            c => c.Id == requestedmemberId && c.PersonId == Convert.ToInt32(personId));
        if (isSameCoach)
        {
            context.Succeed(requirement);
        }

        return;
    }
}