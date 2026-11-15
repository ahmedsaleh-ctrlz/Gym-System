using System.Security.Claims;

using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;

namespace Gym.Application.SubcutaneousTests.Common;

public class TestIdentityServiceTests
{
    private readonly TestIdentityService _sut = new();

    [Fact]
    public async Task CreateUserAsync_ShouldReturnUserId_AndStoreUser_WhenValid()
    {
        var result = await _sut.CreateUserAsync("newuser@gym.com", "P@ssword123", Role.Member, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var user = await _sut.GetUserByIdAsync(result.Value!);
        Assert.True(user.IsSuccess);
        Assert.Equal("newuser@gym.com", user.Value.Email);
        Assert.Equal(10, user.Value.PersonId);
        Assert.Contains(Role.Member.ToString(), user.Value.Roles);

        var username = await _sut.GetUserNameByIdAsync(result.Value!, CancellationToken.None);
        Assert.Equal("newuser", username);

        var email = await _sut.GetEmailByPersonIdAsync(10, CancellationToken.None);
        Assert.True(email.IsSuccess);
        Assert.Equal("newuser@gym.com", email.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        await _sut.CreateUserAsync("duplicate@gym.com", "Password1", Role.Member, 1, CancellationToken.None);

        var result = await _sut.CreateUserAsync("duplicate@gym.com", "Password2", Role.Coach, 2, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.TopError.Type);
        Assert.Equal("UserAlreadyExists", result.TopError.Code);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnError_WhenFailCreateUserIsTrue()
    {
        _sut.FailCreateUser = true;

        var result = await _sut.CreateUserAsync("fail@gym.com", "Password123", Role.Member, 1, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("CreateUserFailed", result.TopError.Code);
    }

    [Fact]
    public async Task CreateExternalUserAsync_ShouldReturnUserId_WithConfirmedEmail_WhenValid()
    {
        var result = await _sut.CreateExternalUserAsync("google@gym.com", Role.Member, 25, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var user = await _sut.GetUserByIdAsync(result.Value);
        Assert.True(user.IsSuccess);
        Assert.Equal("google@gym.com", user.Value.Email);
        Assert.Equal(25, user.Value.PersonId);

        var authResult = await _sut.AuthenticateAsync("google@gym.com", "any-pwd", CancellationToken.None);
        Assert.True(authResult.IsSuccess);
    }

    [Fact]
    public async Task CreateExternalUserAsync_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        await _sut.CreateExternalUserAsync("exists@gym.com", Role.Member, 1, CancellationToken.None);

        var result = await _sut.CreateExternalUserAsync("exists@gym.com", Role.Coach, 2, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("UserAlreadyExists", result.TopError.Code);
    }

    [Fact]
    public async Task CreateExternalUserAsync_ShouldReturnError_WhenFailCreateExternalUserIsTrue()
    {
        _sut.FailCreateExternalUser = true;

        var result = await _sut.CreateExternalUserAsync("externalfail@gym.com", Role.Member, 1, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("CreateUserFailed", result.TopError.Code);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldTrackDeletedPersonId_AndRemoveUser()
    {
        var createResult = await _sut.CreateUserAsync("delete@gym.com", "Pwd123", Role.Member, 50, CancellationToken.None);
        Assert.True(createResult.IsSuccess);

        var deleteResult = await _sut.DeleteUserAsync(50, CancellationToken.None);

        Assert.True(deleteResult.IsSuccess);
        Assert.Contains(50, _sut.DeletedPersonIds);

        var user = await _sut.GetUserByIdAsync(createResult.Value!);
        Assert.True(user.IsError);
        Assert.Equal("UserNotFound", user.TopError.Code);

        var emailResult = await _sut.GetEmailByPersonIdAsync(50, CancellationToken.None);
        Assert.True(emailResult.IsError);
        Assert.Equal("EmailNotFound", emailResult.TopError.Code);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var username = await _sut.GetUserNameByIdAsync("nonexistent-id", CancellationToken.None);

        Assert.Null(username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldAutoProvisionUser_WhenUserNotSeeded()
    {
        var result = await _sut.AuthenticateAsync("autoprovision@gym.com", "anypassword", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("autoprovision@gym.com", result.Value.Email);
        Assert.Equal("auth-user", result.Value.UserId);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnError_WhenFailAuthenticateIsTrue()
    {
        _sut.FailAuthenticate = true;

        var result = await _sut.AuthenticateAsync("user@gym.com", "pwd", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("InvalidCredentials", result.TopError.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnConflict_WhenPasswordIsIncorrect()
    {
        var createResult = await _sut.CreateUserAsync("seeded@gym.com", "CorrectPassword", Role.Member, 1, CancellationToken.None);
        _sut.SeedEmailConfirmed(createResult.Value!, true);

        var result = await _sut.AuthenticateAsync("seeded@gym.com", "WrongPassword", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Invalid_Login_Attempt", result.TopError.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnConflict_WhenEmailIsNotConfirmed()
    {
        await _sut.CreateUserAsync("unconfirmed@gym.com", "Password123", Role.Member, 1, CancellationToken.None);

        var result = await _sut.AuthenticateAsync("unconfirmed@gym.com", "Password123", CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Email_Not_Confirmed", result.TopError.Code);
    }

    [Fact]
    public async Task FindByExternalLoginAsync_ShouldReturnUser_WhenLinked()
    {
        var createResult = await _sut.CreateUserAsync("external@gym.com", "Pwd", Role.Member, 1, CancellationToken.None);
        _sut.SeedExternalLogin(createResult.Value!, "Google", "google-sub-123");

        var user = await _sut.FindByExternalLoginAsync("Google", "google-sub-123", CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal(createResult.Value, user.UserId);
        Assert.Equal("external@gym.com", user.Email);
    }

    [Fact]
    public async Task FindByExternalLoginAsync_ShouldReturnNull_WhenNotLinked()
    {
        var user = await _sut.FindByExternalLoginAsync("Google", "unlinked-key", CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task FindUserByEmailAsync_ShouldReturnUser_WhenExists()
    {
        await _sut.CreateUserAsync("findme@gym.com", "Pwd", Role.Member, 5, CancellationToken.None);

        var user = await _sut.FindUserByEmailAsync("findme@gym.com", CancellationToken.None);

        Assert.NotNull(user);
        Assert.Equal("findme@gym.com", user.Email);
        Assert.Equal(5, user.PersonId);
    }

    [Fact]
    public async Task FindUserByEmailAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        var user = await _sut.FindUserByEmailAsync("notfound@gym.com", CancellationToken.None);

        Assert.Null(user);
    }

    [Fact]
    public async Task LinkExternalLoginAsync_ShouldLinkLogin_AndSetEmailConfirmed_WhenValid()
    {
        var createResult = await _sut.CreateUserAsync("link@gym.com", "Pwd", Role.Member, 8, CancellationToken.None);

        var result = await _sut.LinkExternalLoginAsync(createResult.Value!, "Google", "prov-key-99", true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("link@gym.com", result.Value.Email);

        var linkedUser = await _sut.FindByExternalLoginAsync("Google", "prov-key-99", CancellationToken.None);
        Assert.NotNull(linkedUser);
        Assert.Equal(createResult.Value, linkedUser.UserId);
    }

    [Fact]
    public async Task LinkExternalLoginAsync_ShouldReturnValidationError_WhenEmailNotVerified()
    {
        var createResult = await _sut.CreateUserAsync("unverifiedlink@gym.com", "Pwd", Role.Member, 9, CancellationToken.None);

        var result = await _sut.LinkExternalLoginAsync(createResult.Value!, "Google", "prov-key-100", false, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Email_Not_Verified", result.TopError.Code);
    }

    [Fact]
    public async Task LinkExternalLoginAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var result = await _sut.LinkExternalLoginAsync("invalid-user-id", "Google", "key-1", true, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task LinkExternalLoginAsync_ShouldReturnConflict_WhenLoginAlreadyLinkedToAnotherUser()
    {
        var userOne = await _sut.CreateUserAsync("first@gym.com", "Pwd", Role.Member, 1, CancellationToken.None);
        var userTwo = await _sut.CreateUserAsync("second@gym.com", "Pwd", Role.Member, 2, CancellationToken.None);

        _sut.SeedExternalLogin(userOne.Value!, "Google", "shared-key");

        var result = await _sut.LinkExternalLoginAsync(userTwo.Value!, "Google", "shared-key", true, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("External_Login_Already_Linked", result.TopError.Code);
    }

    [Fact]
    public async Task LinkExternalLoginAsync_ShouldReturnError_WhenFailLinkExternalLoginIsTrue()
    {
        _sut.FailLinkExternalLogin = true;

        var result = await _sut.LinkExternalLoginAsync("any-id", "Google", "key-1", true, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("LinkExternalLoginFailed", result.TopError.Code);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var result = await _sut.GetUserByIdAsync("unknown-id");

        Assert.True(result.IsError);
        Assert.Equal("UserNotFound", result.TopError.Code);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnError_WhenFailGetUserByIdIsTrue()
    {
        var createResult = await _sut.CreateUserAsync("testfail@gym.com", "Pwd", Role.Member, 1, CancellationToken.None);
        _sut.FailGetUserById = true;

        var result = await _sut.GetUserByIdAsync(createResult.Value!);

        Assert.True(result.IsError);
        Assert.Equal("UserNotFound", result.TopError.Code);
    }

    [Fact]
    public async Task GetEmailByPersonIdAsync_ShouldReturnError_WhenFailGetEmailIsTrue()
    {
        _sut.SeedEmail(100, "seeded@gym.com");
        _sut.FailGetEmailByPersonId = true;

        var result = await _sut.GetEmailByPersonIdAsync(100, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("EmailNotFound", result.TopError.Code);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldUpdatePassword_WhenValid()
    {
        var createResult = await _sut.CreateUserAsync("pwduser@gym.com", "OldPassword", Role.Member, 60, CancellationToken.None);
        _sut.SeedEmailConfirmed(createResult.Value!, true);

        var updateResult = await _sut.UpdatePasswordAsync(60, "OldPassword", "NewPassword123");

        Assert.True(updateResult.IsSuccess);

        var authResult = await _sut.AuthenticateAsync("pwduser@gym.com", "NewPassword123", CancellationToken.None);
        Assert.True(authResult.IsSuccess);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldReturnNotFound_WhenPersonIdDoesNotExist()
    {
        var result = await _sut.UpdatePasswordAsync(999, "Old", "New");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldReturnConflict_WhenCurrentPasswordIncorrect()
    {
        await _sut.CreateUserAsync("wrongpwd@gym.com", "RealPassword", Role.Member, 61, CancellationToken.None);

        var result = await _sut.UpdatePasswordAsync(61, "WrongCurrentPassword", "NewPassword");

        Assert.True(result.IsError);
        Assert.Equal("Invalid_Current_Password", result.TopError.Code);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldReturnError_WhenFailUpdatePasswordIsTrue()
    {
        await _sut.CreateUserAsync("failpwd@gym.com", "RealPassword", Role.Member, 62, CancellationToken.None);
        _sut.FailUpdatePassword = true;

        var result = await _sut.UpdatePasswordAsync(62, "RealPassword", "NewPassword");

        Assert.True(result.IsError);
        Assert.Equal("UpdatePasswordFailed", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlAsync_ShouldGenerateUrl_AndAllowConfirmation()
    {
        var createResult = await _sut.CreateUserAsync("confirm@gym.com", "Pwd123", Role.Member, 70, CancellationToken.None);
        var userId = createResult.Value!;

        var urlResult = await _sut.GenerateEmailConfirmationUrlAsync(userId);

        Assert.True(urlResult.IsSuccess);
        Assert.Contains($"userId={userId}", urlResult.Value);
        Assert.Contains("token=", urlResult.Value);

        var token = urlResult.Value.Split("token=")[1];
        var confirmResult = await _sut.ConfirmEmailAsync(userId, token);

        Assert.True(confirmResult.IsSuccess);

        var alreadyConfirmedResult = await _sut.GenerateEmailConfirmationUrlAsync(userId);
        Assert.True(alreadyConfirmedResult.IsError);
        Assert.Equal("Email_Already_Confirmed", alreadyConfirmedResult.TopError.Code);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var result = await _sut.GenerateEmailConfirmationUrlAsync("invalid-id");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlAsync_ShouldReturnError_WhenFailGenerateConfirmationUrlIsTrue()
    {
        var createResult = await _sut.CreateUserAsync("urlfail@gym.com", "Pwd123", Role.Member, 71, CancellationToken.None);
        _sut.FailGenerateConfirmationUrl = true;

        var result = await _sut.GenerateEmailConfirmationUrlAsync(createResult.Value!);

        Assert.True(result.IsError);
        Assert.Equal("GenerateConfirmationUrlFailed", result.TopError.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var result = await _sut.ConfirmEmailAsync("nonexistent-user", "any-token");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnValidationError_WhenTokenIsIncorrect()
    {
        var createResult = await _sut.CreateUserAsync("tokencheck@gym.com", "Pwd", Role.Member, 72, CancellationToken.None);
        await _sut.GenerateEmailConfirmationUrlAsync(createResult.Value!);

        var result = await _sut.ConfirmEmailAsync(createResult.Value!, "completely-wrong-token");

        Assert.True(result.IsError);
        Assert.Equal("Invalid_Token", result.TopError.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnError_WhenFailConfirmEmailIsTrue()
    {
        var createResult = await _sut.CreateUserAsync("confirmfail@gym.com", "Pwd", Role.Member, 73, CancellationToken.None);
        _sut.FailConfirmEmail = true;

        var result = await _sut.ConfirmEmailAsync(createResult.Value!, "token");

        Assert.True(result.IsError);
        Assert.Equal("ConfirmEmailFailed", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlByEmailAsync_ShouldGenerateUrl_WhenValid()
    {
        var createResult = await _sut.CreateUserAsync("urlbyemail@gym.com", "Pwd123", Role.Member, 75, CancellationToken.None);

        var result = await _sut.GenerateEmailConfirmationUrlByEmailAsync("urlbyemail@gym.com");

        Assert.True(result.IsSuccess);
        Assert.Contains(createResult.Value!, result.Value);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlByEmailAsync_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        var result = await _sut.GenerateEmailConfirmationUrlByEmailAsync("notregistered@gym.com");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateEmailConfirmationUrlByEmailAsync_ShouldReturnConflict_WhenAlreadyConfirmed()
    {
        var createResult = await _sut.CreateUserAsync("alreadyconf@gym.com", "Pwd123", Role.Member, 76, CancellationToken.None);
        _sut.SeedEmailConfirmed(createResult.Value!, true);

        var result = await _sut.GenerateEmailConfirmationUrlByEmailAsync("alreadyconf@gym.com");

        Assert.True(result.IsError);
        Assert.Equal("Email_Already_Confirmed", result.TopError.Code);
    }

    [Fact]
    public async Task ResetPasswordFlow_ShouldGenerateToken_AndResetPasswordSuccessfully()
    {
        var createResult = await _sut.CreateUserAsync("forgot@gym.com", "InitialPassword", Role.Member, 80, CancellationToken.None);
        _sut.SeedEmailConfirmed(createResult.Value!, true);

        var tokenResult = await _sut.GenerateResetTokenAsync("forgot@gym.com");
        Assert.True(tokenResult.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(tokenResult.Value));

        var resetResult = await _sut.ResetPasswordAsync("forgot@gym.com", tokenResult.Value, "BrandNewPassword456");
        Assert.True(resetResult.IsSuccess);

        var authResult = await _sut.AuthenticateAsync("forgot@gym.com", "BrandNewPassword456", CancellationToken.None);
        Assert.True(authResult.IsSuccess);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        var result = await _sut.GenerateResetTokenAsync("nobody@gym.com");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GenerateResetTokenAsync_ShouldReturnError_WhenFailGenerateResetTokenIsTrue()
    {
        await _sut.CreateUserAsync("tokengenfail@gym.com", "Pwd", Role.Member, 81, CancellationToken.None);
        _sut.FailGenerateResetToken = true;

        var result = await _sut.GenerateResetTokenAsync("tokengenfail@gym.com");

        Assert.True(result.IsError);
        Assert.Equal("GenerateResetTokenFailed", result.TopError.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        var result = await _sut.ResetPasswordAsync("nonexistent@gym.com", "token", "newpwd");

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnConflict_WhenResetTokenIsInvalid()
    {
        await _sut.CreateUserAsync("badreset@gym.com", "Pwd", Role.Member, 82, CancellationToken.None);
        await _sut.GenerateResetTokenAsync("badreset@gym.com");

        var result = await _sut.ResetPasswordAsync("badreset@gym.com", "wrong-reset-token", "NewPassword");

        Assert.True(result.IsError);
        Assert.Equal("Invalid_Reset_Token", result.TopError.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnError_WhenFailResetPasswordIsTrue()
    {
        await _sut.CreateUserAsync("resetfail@gym.com", "Pwd", Role.Member, 83, CancellationToken.None);
        _sut.FailResetPassword = true;

        var result = await _sut.ResetPasswordAsync("resetfail@gym.com", "token", "NewPassword");

        Assert.True(result.IsError);
        Assert.Equal("ResetPasswordFailed", result.TopError.Code);
    }

    [Fact]
    public async Task GetUserIdByPersonIdAsync_ShouldReturnUserId_WhenUserExists()
    {
        var createResult = await _sut.CreateUserAsync("personlookup@gym.com", "Pwd", Role.Member, 90, CancellationToken.None);

        var result = await _sut.GetUserIdByPersonIdAsync(90);

        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task GetUserIdByPersonIdAsync_ShouldReturnFallbackUserId_WhenEmailSeeded()
    {
        _sut.SeedEmail(91, "seededperson@gym.com");

        var result = await _sut.GetUserIdByPersonIdAsync(91);

        Assert.True(result.IsSuccess);
        Assert.Equal("user-91", result.Value);
    }

    [Fact]
    public async Task GetUserIdByPersonIdAsync_ShouldReturnNotFound_WhenPersonIdUnknown()
    {
        var result = await _sut.GetUserIdByPersonIdAsync(9999);

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GetUserIdByPersonIdAsync_ShouldReturnError_WhenFailGetUserIdByPersonIdIsTrue()
    {
        await _sut.CreateUserAsync("failgetperson@gym.com", "Pwd", Role.Member, 92, CancellationToken.None);
        _sut.FailGetUserIdByPersonId = true;

        var result = await _sut.GetUserIdByPersonIdAsync(92);

        Assert.True(result.IsError);
        Assert.Equal("User_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task GetUsersIdsByPersonIdsAsync_ShouldReturnMatchingUserIds()
    {
        var user1 = await _sut.CreateUserAsync("p1@gym.com", "Pwd", Role.Member, 101, CancellationToken.None);
        var user2 = await _sut.CreateUserAsync("p2@gym.com", "Pwd", Role.Member, 102, CancellationToken.None);
        await _sut.CreateUserAsync("p3@gym.com", "Pwd", Role.Member, 103, CancellationToken.None);

        var result = await _sut.GetUsersIdsByPersonIdsAsync([101, 102, 999], CancellationToken.None);

        Assert.True(result.IsSuccess);
        var list = result.Value.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(user1.Value, list);
        Assert.Contains(user2.Value, list);
    }

    [Fact]
    public async Task GetUsersIdsByRoleAsync_ShouldReturnOnlyUsersWithSpecifiedRole()
    {
        var coach = await _sut.CreateUserAsync("coach@gym.com", "Pwd", Role.Coach, 110, CancellationToken.None);
        var member1 = await _sut.CreateUserAsync("member1@gym.com", "Pwd", Role.Member, 111, CancellationToken.None);
        var member2 = await _sut.CreateUserAsync("member2@gym.com", "Pwd", Role.Member, 112, CancellationToken.None);

        var memberResult = await _sut.GetUsersIdsByRoleAsync(Role.Member, CancellationToken.None);
        Assert.True(memberResult.IsSuccess);
        var memberList = memberResult.Value.ToList();
        Assert.Equal(2, memberList.Count);
        Assert.Contains(member1.Value, memberList);
        Assert.Contains(member2.Value, memberList);
        Assert.DoesNotContain(coach.Value, memberList);

        var coachResult = await _sut.GetUsersIdsByRoleAsync(Role.Coach, CancellationToken.None);
        Assert.True(coachResult.IsSuccess);
        var coachList = coachResult.Value.ToList();
        Assert.Single(coachList);
        Assert.Equal(coach.Value, coachList[0]);
    }

    [Fact]
    public async Task GetEmailsByPersonIdsAsync_ShouldReturnMatchingEmails()
    {
        _sut.SeedEmail(201, "first@gym.com");
        _sut.SeedEmail(202, "second@gym.com");
        _sut.SeedEmail(203, "third@gym.com");

        var result = await _sut.GetEmailsByPersonIdsAsync([201, 203, 999], CancellationToken.None);

        Assert.True(result.IsSuccess);
        var list = result.Value.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains("first@gym.com", list);
        Assert.Contains("third@gym.com", list);
    }

    [Fact]
    public async Task SeedUser_ShouldPopulateUserAndEmailAndAllowAuthentication()
    {
        var user = new AppUserDto("custom-id", 300, "seededuser@gym.com", [Role.Admin.ToString()], [new Claim(ClaimTypes.NameIdentifier, "custom-id")]);

        _sut.SeedUser(user);
        _sut.SeedPassword("custom-id", "AdminSecret123");

        var fetched = await _sut.GetUserByIdAsync("custom-id");
        Assert.True(fetched.IsSuccess);
        Assert.Equal("seededuser@gym.com", fetched.Value.Email);
        Assert.Equal(300, fetched.Value.PersonId);
        Assert.Contains(Role.Admin.ToString(), fetched.Value.Roles);

        var authResult = await _sut.AuthenticateAsync("seededuser@gym.com", "AdminSecret123", CancellationToken.None);
        Assert.True(authResult.IsSuccess);
        Assert.Equal("custom-id", authResult.Value.UserId);

        var emailResult = await _sut.GetEmailByPersonIdAsync(300, CancellationToken.None);
        Assert.True(emailResult.IsSuccess);
        Assert.Equal("seededuser@gym.com", emailResult.Value);
    }
}
