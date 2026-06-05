using Gym.Application.Common.Interfaces;
using System.Security.Claims;

namespace Gym.Api.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public string? Id => 
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? PersonId => 
        httpContextAccessor.HttpContext?.User?.FindFirstValue("person_id");
     public string? Role => 
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
}
