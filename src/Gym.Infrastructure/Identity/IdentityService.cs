using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Common.Result;
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
            return Error.Conflict("UserAlreadyExists", $"A user with the email '{email}' already exists.");
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
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PersonId == PersonId);

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

    public async Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }
}
