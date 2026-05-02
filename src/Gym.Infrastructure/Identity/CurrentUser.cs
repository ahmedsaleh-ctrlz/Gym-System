using System.Security.Claims;
using Gym.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Gym.Infrastructure.Identity;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
