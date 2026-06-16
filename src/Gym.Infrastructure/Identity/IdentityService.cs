using Gym.Application.Common.Helpers;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Identity;

public class IdentityService(UserManager<AppUser> userManager) : IIdentityService
{
    public async Task<Result<string?>> CreateUserAsync(string email, string password, Role role, int personId, CancellationToken ct)
    {
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Error.Conflict("UserAlreadyExists", $"A user with the email '{Utility.MaskEmail(email)}' already exists.");
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            PersonId = personId
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        var roleResult = await userManager.AddToRoleAsync(user, role.ToString());

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            var errors = roleResult.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        return user.Id;
    }

    public async Task<Result<Deleted>> DeleteUserAsync(int PersonId, CancellationToken ct)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PersonId == PersonId, ct);

        if (user is null)
        {
            return Error.NotFound("UserNotFound", $"User with ID '{PersonId}' not found.");
        }

        var deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            return deleteResult.Errors
                .Select(e => Error.Conflict(e.Code, e.Description))
                .ToList();
        }

        return Result.Deleted;
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(nameof(userId));

        var roles = await userManager.GetRolesAsync(user);

        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.PersonId, user.Email!, roles, claims);
    }

    public async Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string passsword, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Error.NotFound("User_Not_Found", $"User with email {Utility.MaskEmail(email)} not found");
        }

        if (!await userManager.CheckPasswordAsync(user, passsword))
        {
            return Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");
        }

        return new AppUserDto(user.Id, user.PersonId, user.Email!, await userManager.GetRolesAsync(user), await userManager.GetClaimsAsync(user));
    }

    public async Task<Result<string>> GetEmailByPersonIdAsync(int personId, CancellationToken ct)
    {
        var user = await userManager.Users.Select(u => new { u.PersonId, u.Email }).FirstOrDefaultAsync(u => u.PersonId == personId, ct);
        if (user is null)
        {
            return Error.NotFound("User_Not_Found", $"User with PersonId {personId} not found");
        }

        return user.Email!;
    }
}