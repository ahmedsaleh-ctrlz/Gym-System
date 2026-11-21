using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Infrastructure.Data;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace Gym.Api.IntegrationTests;

public class GymWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationTests_{Guid.NewGuid():N}";
    private readonly SemaphoreSlim _dbLock = new(1, 1);
    private bool _isDbInitialized;
    private SqliteConnection? _connection;

    public async Task EnsureDatabaseCreatedAsync()
    {
        if (_isDbInitialized)
        {
            return;
        }

        await _dbLock.WaitAsync();
        try
        {
            if (_isDbInitialized)
            {
                return;
            }

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = ["Admin", "Member", "Coach"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            _isDbInitialized = true;
        }
        finally
        {
            _dbLock.Release();
        }
    }

    public string CreateJwtToken(string userId, string email, int personId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
        };

        if (personId > 0)
        {
            claims.Add(new("person_id", personId.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("GymSystemSuperSecretIntegrationTestKey12345!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "GymApi",
            Audience = "GymClient",
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(string userId, string email, int personId, params string[] roles)
    {
        var client = CreateClient();
        var token = CreateJwtToken(userId, email, personId, roles);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server = . ; Database = GymDB ; Trusted_Connection=True; MultipleActiveResultSets = true ; TrustServerCertificate = True;",
                ["JwtSettings:Secret"] = "GymSystemSuperSecretIntegrationTestKey12345!",
                ["JwtSettings:Issuer"] = "GymApi",
                ["JwtSettings:Audience"] = "GymClient",
                ["JwtSettings:TokenExpirationInMinutes"] = "60",
                ["AppSettings:AllowedOrigins:0"] = "https://localhost:7022",
                ["Stripe:SecretKey"] = "sk_test_mock_stripe_key",
                ["Stripe:PublishableKey"] = "pk_test_mock_stripe_key",
                ["Stripe:WebhookSecret"] = "whsec_mock",
                ["Email:ApiKey"] = "test-email-key",
                ["Email:FromEmail"] = "noreply@gymsystem.com",
                ["Email:FromName"] = "Gym System",
                ["Authentication:Google:ClientId"] = "test-google-client-id.apps.googleusercontent.com",
                ["Authentication:Google:ClientSecret"] = "test-google-client-secret",
                ["Serilog:WriteTo:1:Args:serverUrl"] = "http://localhost:5341"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing AppDbContext and DbContextOptions
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(AppDbContext)
                         || d.ServiceType.FullName?.Contains("AppDbContext") == true
                         || d.ServiceType.FullName?.Contains("DbContextOptions") == true)
                .ToList();

            foreach (var descriptor in dbDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove Hangfire background server hosted service if registered to avoid SQL connection
            var hangfireHostedServices = services
                .Where(d => d.ImplementationType?.FullName?.Contains("Hangfire") == true)
                .ToList();
            foreach (var s in hangfireHostedServices)
            {
                services.Remove(s);
            }

            // Setup SQLite in-memory with unique database name per factory
            _connection = new SqliteConnection($"Data Source={_dbName};Mode=Memory;Cache=Shared");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Replace external dependencies with substitutes
            services.AddScoped(_ => Substitute.For<IEmailSender>());
            services.AddScoped(_ => Substitute.For<IEmailBackgroundJobs>());
            services.AddScoped(_ => Substitute.For<INotificationBackgroundJobs>());
            services.AddScoped(_ => Substitute.For<IStripePaymentService>());
            services.AddScoped(_ => Substitute.For<IAiService>());
            var mockRecurringJobManager = Substitute.For<IRecurringJobManager>();
            services.AddSingleton(mockRecurringJobManager);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
