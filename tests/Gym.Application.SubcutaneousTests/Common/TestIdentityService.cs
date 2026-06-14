using System.Security.Claims;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestIdentityService : IIdentityService
{
    private readonly Dictionary<int, string> _emailsByPersonId = [];
    private readonly Dictionary<string, AppUserDto> _usersById = [];
    private readonly Dictionary<string, string> _userNamesById = [];

    public bool FailCreateUser { get; set; }
    public bool FailAuthenticate { get; set; }
    public bool FailGetUserById { get; set; }
    public bool FailGetEmailByPersonId { get; set; }
    public List<int> DeletedPersonIds { get; } = [];

    public Task<Result<string?>> CreateUserAsync(string email, string password, Role role, int personId, CancellationToken ct)
    {
        if (FailCreateUser)
        {
            return Task.FromResult<Result<string?>>(Error.Conflict("CreateUserFailed", "Create user failed."));
        }

        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var user = new AppUserDto(userId, personId, email, [role.ToString()], claims);

        _emailsByPersonId[personId] = email;
        _usersById[userId] = user;
        _userNamesById[userId] = email.Split('@')[0];

        return Task.FromResult<Result<string?>>(userId);
    }

    public Task<Result<Deleted>> DeleteUserAsync(int personId, CancellationToken ct)
    {
        DeletedPersonIds.Add(personId);
        return Task.FromResult<Result<Deleted>>(Result.Deleted);
    }

    public Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct)
    {
        _userNamesById.TryGetValue(userId, out var value);
        return Task.FromResult<string?>(value);
    }

    public Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        if (FailAuthenticate)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Validation("InvalidCredentials", "Invalid credentials."));
        }

        var user = _usersById.Values.FirstOrDefault(x => x.Email == email)
            ?? new AppUserDto("auth-user", null, email, [Role.Member.ToString()], [new Claim(ClaimTypes.NameIdentifier, "auth-user")]);

        _usersById[user.UserId] = user;
        return Task.FromResult<Result<AppUserDto>>(user);
    }

    public Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        if (FailGetUserById || !_usersById.TryGetValue(userId, out var user))
        {
            return Task.FromResult<Result<AppUserDto>>(Error.NotFound("UserNotFound", "User not found."));
        }

        return Task.FromResult<Result<AppUserDto>>(user);
    }

    public Task<Result<string>> GetEmailByPersonIdAsync(int personId, CancellationToken ct)
    {
        if (FailGetEmailByPersonId || !_emailsByPersonId.TryGetValue(personId, out var email))
        {
            return Task.FromResult<Result<string>>(Error.NotFound("EmailNotFound", "Email not found."));
        }

        return Task.FromResult<Result<string>>(email);
    }

    public void SeedUser(AppUserDto user)
    {
        _usersById[user.UserId] = user;

        if (user.PersonId.HasValue)
        {
            _emailsByPersonId[user.PersonId.Value] = user.Email;
        }

        _userNamesById[user.UserId] = user.Email.Split('@')[0];
    }

    public void SeedEmail(int personId, string email)
    {
        _emailsByPersonId[personId] = email;
    }
}
