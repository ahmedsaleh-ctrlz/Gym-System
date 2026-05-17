using Gym.Application.Common.Interfaces;
using Gym.Infrastructure.Data;
using Gym.Infrastructure.Data.Interceptors;
using Gym.Infrastructure.Identity;
using Gym.Infrastructure.Identity.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gym.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHttpContextAccessor();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<AppDbContextInitailiser>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetService<ISaveChangesInterceptor>()!);
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ITokenProvider, TokenProvider>();

        services.AddScoped<IAuthorizationHandler, SameCoachHandler>();
        services.AddScoped<IAuthorizationHandler, SameMemberOrAdminRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, SameMemberOrCoachOrAdminRequirementHandler>();

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();

        services.AddDataProtection();

        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5), // L2, L3
            LocalCacheExpiration = TimeSpan.FromSeconds(30), // L1
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.SameCoach, policy => policy.AddRequirements(new SameCoachRequirement()));
            options.AddPolicy(Policies.SameMemberOrAdmin, policy => policy.AddRequirements(new SameMemberOrAdminRequirement()));
            options.AddPolicy(Policies.SameMemberOrCoachOrAdmin, policy => policy.AddRequirements(new SameMemberOrCoachOrAdminRequirement()));

        });

        return services;
    }

}
