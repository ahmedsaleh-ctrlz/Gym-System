using Gym.Domain.Coachs;
using Gym.Domain.Common.Constants.Enums;
using Gym.Domain.Members;
using Gym.Domain.People;
using Gym.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gym.Infrastructure.Data;

public class AppDbContextInitailiser(ILogger<AppDbContextInitailiser> logger,
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager)
{

    private readonly ILogger<AppDbContextInitailiser> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }


    public async Task TrySeedAsync()
    {
        var adminRole = new IdentityRole(nameof(Role.Admin));
        var memberRole = new IdentityRole(nameof(Role.Member));
        var coachRole = new IdentityRole(nameof(Role.Coach));

        await CheckRoleAndCreateIfNotExist(new IdentityRole(nameof(Role.Admin)));
        await CheckRoleAndCreateIfNotExist(new IdentityRole(nameof(Role.Coach)));
        await CheckRoleAndCreateIfNotExist(new IdentityRole(nameof(Role.Member)));

        await _context.SaveChangesAsync();


        var admin = new AppUser
        {
           
            Email = "A@gmail.com",
            UserName = "Ahmedsaleh",
            EmailConfirmed = true 
        };

        if (_userManager.Users.All(u => u.Email != admin.Email))
        {
            var result = await _userManager.CreateAsync(admin, "123456");

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));
            }

            if (!string.IsNullOrWhiteSpace(adminRole.Name))
            {
                await _userManager.AddToRolesAsync(admin, [adminRole.Name!]);
            }
        }

    }
    private async Task CheckRoleAndCreateIfNotExist(IdentityRole role)
    {
        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }
    }


}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<AppDbContextInitailiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}
