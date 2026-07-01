using System.Linq.Expressions;
using System.Security.Principal;

using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

namespace Gym.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<string?>> CreateUserAsync(string email, string password, Role role, int personId, CancellationToken ct);
    Task<Result<Deleted>> DeleteUserAsync(int personId, CancellationToken ct);

    Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

    Task<Result<string>> GetEmailByPersonIdAsync(int personId, CancellationToken ct);
    Task<Result<Updated>> UpdatePasswordAsync(int personId, string currentPassword, string newPassword);
    Task<Result<string>> GenerateEmailConfirmationUrlAsync(string userId);
    Task<Result<Updated>> ConfirmEmailAsync(string userId, string token);
    Task<Result<string>> GenerateEmailConfirmationUrlByEmailAsync(string email);
}