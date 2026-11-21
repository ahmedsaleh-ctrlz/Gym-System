using Gym.Domain.Identity;

using Hangfire.Dashboard;

namespace Gym.Api.Infrastructure;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole(nameof(Role.Admin));
    }
}
