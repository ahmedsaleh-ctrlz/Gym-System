using Gym.Application;
using Gym.Application.Common.Interfaces;
using Gym.Infrastructure.Data;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class SubcutaneousTestContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    private SubcutaneousTestContext(
        SqliteConnection connection,
        ServiceProvider provider,
        IServiceScope scope)
    {
        _connection = connection;
        _provider = provider;
        _scope = scope;
    }

    public IMediator Mediator => _scope.ServiceProvider.GetRequiredService<IMediator>();
    public AppDbContext DbContext => _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    public TestCurrentUser CurrentUser => _scope.ServiceProvider.GetRequiredService<TestCurrentUser>();
    public TestIdentityService IdentityService => _scope.ServiceProvider.GetRequiredService<TestIdentityService>();
    public TestTokenProvider TokenProvider => _scope.ServiceProvider.GetRequiredService<TestTokenProvider>();
    public HybridCache Cache => _scope.ServiceProvider.GetRequiredService<HybridCache>();

    public static async Task<SubcutaneousTestContext> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicaiton();
        services.AddDistributedMemoryCache();
        services.AddHybridCache();

        services.AddSingleton<TestCurrentUser>();
        services.AddSingleton<IUser>(sp => sp.GetRequiredService<TestCurrentUser>());
        services.AddSingleton<TestIdentityService>();
        services.AddSingleton<IIdentityService>(sp => sp.GetRequiredService<TestIdentityService>());
        services.AddSingleton<TestTokenProvider>();
        services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<TestTokenProvider>());

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        return new SubcutaneousTestContext(connection, provider, scope);
    }

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        _provider.Dispose();
        return _connection.DisposeAsync();
    }
}