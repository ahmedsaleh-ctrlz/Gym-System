using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gym.Infrastructure.Identity;

public class TokenProvider(IConfiguration configuration , IAppDbContext context) : ITokenProvider
{
    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
    {
        var tokenResult = await _CreateAsync(user, ct);

        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }

    public IEnumerable<Claim> GetClaimsFromExpiredToken(string ExpiredToken)
    {
        var tokenHelper = new JwtSecurityTokenHandler();

        var token = tokenHelper.ReadJwtToken(ExpiredToken);

        return token.Claims;

    }

    private async Task<Result<TokenResponse>> _CreateAsync(AppUserDto user, CancellationToken ct)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["Secret"]!;

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub , user.UserId!),
            new (JwtRegisteredClaimNames.Email , user.Email!)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new(ClaimTypes.Role,role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
               new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
               SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(descriptor);

        var oldRefreshTokens = await context.RefreshTokens
              .Where(rt => rt.UserId == user.UserId)
              .ExecuteDeleteAsync(ct);

        var refreshTokenResult = RefreshToken.Create(GenerateRefreshToken(), user.UserId, DateTime.UtcNow.AddDays(7));

        if (refreshTokenResult.IsError)
        {
            return refreshTokenResult.Errors;
        }

        await context.RefreshTokens.AddAsync(refreshTokenResult.Value,ct);

        await context.SaveChangesAsync(ct);

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = refreshTokenResult.Value.Token
        };
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    
}
