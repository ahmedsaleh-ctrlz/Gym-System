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
    private readonly Dictionary<string, string> _passwordsByUserId = [];
    private readonly Dictionary<string, bool> _emailConfirmedByUserId = [];
    private readonly Dictionary<string, string> _resetTokensByEmail = [];
    private readonly Dictionary<string, string> _confirmationTokensByUserId = [];
    private readonly Dictionary<(string Provider, string Key), string> _externalLogins = [];

    public bool FailCreateUser { get; set; }
    public bool FailCreateExternalUser { get; set; }
    public bool FailAuthenticate { get; set; }
    public bool FailGetUserById { get; set; }
    public bool FailGetEmailByPersonId { get; set; }
    public bool FailUpdatePassword { get; set; }
    public bool FailConfirmEmail { get; set; }
    public bool FailGenerateConfirmationUrl { get; set; }
    public bool FailGenerateResetToken { get; set; }
    public bool FailResetPassword { get; set; }
    public bool FailLinkExternalLogin { get; set; }
    public bool FailGetUserIdByPersonId { get; set; }
    public List<int> DeletedPersonIds { get; } = [];

    public Task<Result<string?>> CreateUserAsync(string email, string password, Role role, int personId, CancellationToken ct)
    {
        if (FailCreateUser)
        {
            return Task.FromResult<Result<string?>>(Error.Conflict("CreateUserFailed", "Create user failed."));
        }

        if (_usersById.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult<Result<string?>>(Error.Conflict("UserAlreadyExists", $"A user with the email '{email}' already exists."));
        }

        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var user = new AppUserDto(userId, personId, email, [role.ToString()], claims);

        _emailsByPersonId[personId] = email;
        _usersById[userId] = user;
        _userNamesById[userId] = email.Split('@')[0];
        _passwordsByUserId[userId] = password;
        _emailConfirmedByUserId[userId] = false;

        return Task.FromResult<Result<string?>>(userId);
    }

    public Task<Result<string>> CreateExternalUserAsync(string email, Role role, int personId, CancellationToken ct)
    {
        if (FailCreateExternalUser)
        {
            return Task.FromResult<Result<string>>(Error.Conflict("CreateUserFailed", "Create user failed."));
        }

        if (_usersById.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult<Result<string>>(Error.Conflict("UserAlreadyExists", $"A user with the email '{email}' already exists."));
        }

        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var user = new AppUserDto(userId, personId, email, [role.ToString()], claims);

        _emailsByPersonId[personId] = email;
        _usersById[userId] = user;
        _userNamesById[userId] = email.Split('@')[0];
        _emailConfirmedByUserId[userId] = true;

        return Task.FromResult<Result<string>>(userId);
    }

    public Task<Result<Deleted>> DeleteUserAsync(int personId, CancellationToken ct)
    {
        DeletedPersonIds.Add(personId);

        var user = _usersById.Values.FirstOrDefault(u => u.PersonId == personId);
        if (user is not null)
        {
            _usersById.Remove(user.UserId);
            _userNamesById.Remove(user.UserId);
            _passwordsByUserId.Remove(user.UserId);
            _emailConfirmedByUserId.Remove(user.UserId);
            _confirmationTokensByUserId.Remove(user.UserId);
        }

        _emailsByPersonId.Remove(personId);

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

        var user = _usersById.Values.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            user = new AppUserDto("auth-user", null, email, [Role.Member.ToString()], [new Claim(ClaimTypes.NameIdentifier, "auth-user")]);
            _usersById[user.UserId] = user;
            _passwordsByUserId[user.UserId] = password;
            _emailConfirmedByUserId[user.UserId] = true;
            return Task.FromResult<Result<AppUserDto>>(user);
        }

        if (_passwordsByUserId.TryGetValue(user.UserId, out var expectedPassword) && expectedPassword != password)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect"));
        }

        if (_emailConfirmedByUserId.TryGetValue(user.UserId, out var isConfirmed) && !isConfirmed)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Conflict("Email_Not_Confirmed", "Email is not confirmed , please confirm your email before logging in"));
        }

        return Task.FromResult<Result<AppUserDto>>(user);
    }

    public Task<AppUserDto?> FindByExternalLoginAsync(string loginProvider, string providerKey, CancellationToken ct)
    {
        if (_externalLogins.TryGetValue((loginProvider, providerKey), out var userId) &&
            _usersById.TryGetValue(userId, out var user))
        {
            return Task.FromResult<AppUserDto?>(user);
        }

        return Task.FromResult<AppUserDto?>(null);
    }

    public Task<AppUserDto?> FindUserByEmailAsync(string email, CancellationToken ct)
    {
        var user = _usersById.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<Result<AppUserDto>> LinkExternalLoginAsync(string userId, string loginProvider, string providerKey, bool emailVerified, CancellationToken ct)
    {
        if (FailLinkExternalLogin)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Conflict("LinkExternalLoginFailed", "Link external login failed."));
        }

        if (!emailVerified)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Validation("Email_Not_Verified", "The external provider has not verified the email."));
        }

        if (!_usersById.TryGetValue(userId, out var user))
        {
            return Task.FromResult<Result<AppUserDto>>(Error.NotFound("User_Not_Found", "The Identity user was not found."));
        }

        if (_externalLogins.TryGetValue((loginProvider, providerKey), out var existingUserId) && existingUserId != userId)
        {
            return Task.FromResult<Result<AppUserDto>>(Error.Conflict("External_Login_Already_Linked", "This external login is linked to another account."));
        }

        _externalLogins[(loginProvider, providerKey)] = userId;
        _emailConfirmedByUserId[userId] = true;

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

    public Task<Result<Updated>> UpdatePasswordAsync(int personId, string currentPassword, string newPassword)
    {
        if (FailUpdatePassword)
        {
            return Task.FromResult<Result<Updated>>(Error.Conflict("UpdatePasswordFailed", "Update password failed."));
        }

        var user = _usersById.Values.FirstOrDefault(u => u.PersonId == personId);
        if (user is null)
        {
            return Task.FromResult<Result<Updated>>(Error.NotFound("User_Not_Found", $"User with PersonId {personId} not found"));
        }

        if (_passwordsByUserId.TryGetValue(user.UserId, out var existingPassword) && existingPassword != currentPassword)
        {
            return Task.FromResult<Result<Updated>>(Error.Conflict("Invalid_Current_Password", "Current password is incorrect"));
        }

        _passwordsByUserId[user.UserId] = newPassword;
        return Task.FromResult<Result<Updated>>(Result.Updated);
    }

    public Task<Result<string>> GenerateEmailConfirmationUrlAsync(string userId)
    {
        if (FailGenerateConfirmationUrl)
        {
            return Task.FromResult<Result<string>>(Error.Failure("GenerateConfirmationUrlFailed", "Failed to generate confirmation URL."));
        }

        if (!_usersById.TryGetValue(userId, out var user))
        {
            return Task.FromResult<Result<string>>(Error.NotFound("User_Not_Found", $"User with ID '{userId}' was not found."));
        }

        if (_emailConfirmedByUserId.TryGetValue(userId, out var confirmed) && confirmed)
        {
            return Task.FromResult<Result<string>>(Error.Conflict("Email_Already_Confirmed", "Email is already confirmed"));
        }

        var token = Guid.NewGuid().ToString("N");
        _confirmationTokensByUserId[userId] = token;

        var url = $"https://localhost/api/v2/identity/confirm-email?userId={userId}&token={token}";
        return Task.FromResult<Result<string>>(url);
    }

    public Task<Result<Updated>> ConfirmEmailAsync(string userId, string token)
    {
        if (FailConfirmEmail)
        {
            return Task.FromResult<Result<Updated>>(Error.Validation("ConfirmEmailFailed", "Failed to confirm email."));
        }

        if (!_usersById.TryGetValue(userId, out var user))
        {
            return Task.FromResult<Result<Updated>>(Error.NotFound("User_Not_Found", $"User with ID '{userId}' was not found."));
        }

        if (_confirmationTokensByUserId.TryGetValue(userId, out var expectedToken) &&
            !string.IsNullOrEmpty(expectedToken) &&
            !expectedToken.Equals(token, StringComparison.Ordinal))
        {
            return Task.FromResult<Result<Updated>>(Error.Validation("Invalid_Token", "Invalid confirmation token"));
        }

        _emailConfirmedByUserId[userId] = true;
        return Task.FromResult<Result<Updated>>(Result.Updated);
    }

    public Task<Result<string>> GenerateEmailConfirmationUrlByEmailAsync(string email)
    {
        if (FailGenerateConfirmationUrl)
        {
            return Task.FromResult<Result<string>>(Error.Failure("GenerateConfirmationUrlFailed", "Failed to generate confirmation URL."));
        }

        var user = _usersById.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return Task.FromResult<Result<string>>(Error.NotFound("User_Not_Found", $"User with email '{email}' was not found."));
        }

        if (_emailConfirmedByUserId.TryGetValue(user.UserId, out var confirmed) && confirmed)
        {
            return Task.FromResult<Result<string>>(Error.Conflict("Email_Already_Confirmed", "Email is already confirmed"));
        }

        var token = Guid.NewGuid().ToString("N");
        _confirmationTokensByUserId[user.UserId] = token;

        var url = $"https://localhost/api/v2/identity/confirm-email?userId={user.UserId}&token={token}";
        return Task.FromResult<Result<string>>(url);
    }

    public Task<Result<string>> GenerateResetTokenAsync(string email)
    {
        if (FailGenerateResetToken)
        {
            return Task.FromResult<Result<string>>(Error.Failure("GenerateResetTokenFailed", "Failed to generate reset token."));
        }

        var user = _usersById.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return Task.FromResult<Result<string>>(Error.NotFound("User_Not_Found", $"User with {email} not exist"));
        }

        var token = Guid.NewGuid().ToString("N");
        _resetTokensByEmail[email] = token;
        return Task.FromResult<Result<string>>(token);
    }

    public Task<Result<Updated>> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        if (FailResetPassword)
        {
            return Task.FromResult<Result<Updated>>(Error.Conflict("ResetPasswordFailed", "Reset password failed."));
        }

        var user = _usersById.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return Task.FromResult<Result<Updated>>(Error.NotFound("User_Not_Found", $"User with {email} not exist"));
        }

        if (_resetTokensByEmail.TryGetValue(email, out var expectedToken) &&
            !string.IsNullOrEmpty(expectedToken) &&
            !expectedToken.Equals(resetToken, StringComparison.Ordinal))
        {
            return Task.FromResult<Result<Updated>>(Error.Conflict("Invalid_Reset_Token", "Invalid reset token."));
        }

        _passwordsByUserId[user.UserId] = newPassword;
        _resetTokensByEmail.Remove(email);
        return Task.FromResult<Result<Updated>>(Result.Updated);
    }

    public Task<Result<string>> GetUserIdByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (FailGetUserIdByPersonId)
        {
            return Task.FromResult<Result<string>>(Error.NotFound("User_Not_Found", "User not found."));
        }

        var user = _usersById.Values.FirstOrDefault(u => u.PersonId == personId);
        if (user is not null)
        {
            return Task.FromResult<Result<string>>(user.UserId);
        }

        if (_emailsByPersonId.TryGetValue(personId, out var email))
        {
            var fallbackUser = new AppUserDto($"user-{personId}", personId, email, [Role.Member.ToString()], [new Claim(ClaimTypes.NameIdentifier, $"user-{personId}")]);
            _usersById[fallbackUser.UserId] = fallbackUser;
            return Task.FromResult<Result<string>>(fallbackUser.UserId);
        }

        return Task.FromResult<Result<string>>(Error.NotFound("User_Not_Found", "User not found."));
    }

    public Task<Result<IEnumerable<string>>> GetUsersIdsByPersonIdsAsync(IEnumerable<int> personIds, CancellationToken cancellationToken = default)
    {
        var set = personIds.ToHashSet();
        var userIds = _usersById.Values
            .Where(u => u.PersonId.HasValue && set.Contains(u.PersonId.Value))
            .Select(u => u.UserId)
            .ToList();

        return Task.FromResult<Result<IEnumerable<string>>>(userIds);
    }

    public Task<Result<IEnumerable<string>>> GetUsersIdsByRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        var roleName = role.ToString();
        var userIds = _usersById.Values
            .Where(u => u.Roles.Contains(roleName))
            .Select(u => u.UserId)
            .ToList();

        return Task.FromResult<Result<IEnumerable<string>>>(userIds);
    }

    public Task<Result<IEnumerable<string>>> GetEmailsByPersonIdsAsync(IEnumerable<int> personIds, CancellationToken cancellationToken = default)
    {
        var set = personIds.ToHashSet();
        var emails = _emailsByPersonId
            .Where(kv => set.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToList();

        return Task.FromResult<Result<IEnumerable<string>>>(emails);
    }

    public void SeedUser(AppUserDto user)
    {
        _usersById[user.UserId] = user;

        if (user.PersonId.HasValue)
        {
            _emailsByPersonId[user.PersonId.Value] = user.Email;
        }

        _userNamesById[user.UserId] = user.Email.Split('@')[0];
        _emailConfirmedByUserId[user.UserId] = true;
    }

    public void SeedEmail(int personId, string email)
    {
        _emailsByPersonId[personId] = email;
    }

    public void SeedPassword(string userId, string password)
    {
        _passwordsByUserId[userId] = password;
    }

    public void SeedEmailConfirmed(string userId, bool confirmed)
    {
        _emailConfirmedByUserId[userId] = confirmed;
    }

    public void SeedExternalLogin(string userId, string loginProvider, string providerKey)
    {
        _externalLogins[(loginProvider, providerKey)] = userId;
    }
}
