
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using System.Linq.Expressions;
using System.Security.Principal;

namespace Gym.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result<string?>> CreateUserAsync(string email, string password,Role role, int personId, CancellationToken ct);
    Task<Result<Deleted>> DeleteUserAsync(int personId, CancellationToken ct);

    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password ,CancellationToken ct);


}
