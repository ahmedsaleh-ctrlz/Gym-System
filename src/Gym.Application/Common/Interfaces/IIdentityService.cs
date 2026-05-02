
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Common.Result;
using System.Linq.Expressions;
using System.Security.Principal;

namespace Gym.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result<string?>> CreateUserAsync(string email, string password,Role role, int personId, CancellationToken ct);
    Task<Result<Deleted>> DeleteUserAsync(string userId, CancellationToken ct);

    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct);


}
