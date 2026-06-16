using System.Security.Claims;

using Gym.Application.Common.Errors;
using Gym.Application.Features.Identity.Queries.GenerateToken;
using Gym.Application.Features.Identity.Queries.RefreshToken;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Identity;

namespace Gym.Application.SubcutaneousTests.Features.Identity;

public class IdentityFeatureTests
{
    [Fact]
    public async Task GenerateTokenQuery_ShouldReturnTokens_WhenAuthenticationSucceeds()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new GenerateTokenQuery("auth@gym.com", "123456"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.AccessToken);
        Assert.NotNull(result.Value.RefreshToken);
    }

    [Fact]
    public async Task GenerateTokenQuery_ShouldReturnError_WhenAuthenticationFails()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        context.IdentityService.FailAuthenticate = true;

        var result = await context.Mediator.Send(new GenerateTokenQuery("auth@gym.com", "wrong"));

        Assert.True(result.IsError);
    }

    [Fact]
    public async Task RefreshTokenQuery_ShouldReturnError_WhenExpiredTokenPrincipalIsInvalid()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        context.TokenProvider.PrincipalFromExpiredToken = null;

        var result = await context.Mediator.Send(new RefreshTokenQuery("refresh-token", "expired-access-token"));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.ExpiredAccessTokenInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public async Task RefreshTokenQuery_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var user = TestDataSeeder.CreateUser("user-1", null, "refresh@gym.com", Role.Member.ToString());
        context.IdentityService.SeedUser(user);
        context.TokenProvider.PrincipalFromExpiredToken = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")]));
        await TestDataSeeder.AddRefreshTokenAsync(context, "user-1");

        var result = await context.Mediator.Send(new RefreshTokenQuery("refresh-token", "expired-access-token"));

        Assert.True(result.IsSuccess);
        Assert.Equal("access-user-1", result.Value.AccessToken);
    }
}