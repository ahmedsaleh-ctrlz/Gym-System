
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Common.Result;
using System.Linq.Expressions;
using System.Security.Principal;

namespace Gym.Application.Common.Interfaces;
public interface IIdentityService
{
    Task<Result<int>> CreateUserAsync(string email, string password,Role role,CancellationToken ct);
    Task<Result<Deleted>> DeleteUserAsync(int userId, CancellationToken ct);

    Task<string?> GetUserNameByIdAsync(int userId, CancellationToken ct);


}
