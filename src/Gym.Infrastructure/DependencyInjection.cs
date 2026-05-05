using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Constants.Enums;
using Gym.Infrastructure.Data;
using Gym.Infrastructure.Data.Interceptors;
using Gym.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;

namespace Gym.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<ISaveChangesInterceptor,AuditableEntityInterceptor>();
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<AppDbContextInitailiser>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetService<ISaveChangesInterceptor>()!);
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

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

        return services;
    }

}
