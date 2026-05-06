using Gym.Application.Common.Interfaces;
using Gym.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

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

        var routeValue = httpContext.HttpContext!.Request.RouteValues["id"]?.ToString();

        if (!int.TryParse(routeValue, out int requestedCoachId))
        {
            context.Fail();
            return ;
        }

        var persronId = context.User.FindFirst("person_id")?.Value;

        if (string.IsNullOrWhiteSpace(persronId))
        {
            context.Fail();
            return;
        }

        var isSameCoach = await dbContext.Coaches.AnyAsync
            (c => c.Id == requestedCoachId && c.PersonId == Convert.ToInt32(persronId));
        if (isSameCoach) 
        {
            context.Succeed(requirement);
        }

        return; 
    }
}