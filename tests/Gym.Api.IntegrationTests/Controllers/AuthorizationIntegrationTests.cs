using System.Net;
using Gym.Domain.Identity;

namespace Gym.Api.IntegrationTests.Controllers;

public sealed class AuthorizationIntegrationTests : IClassFixture<GymWebApplicationFactory>, IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;

    public AuthorizationIntegrationTests(GymWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("/api/v1/dashboard/stats")]
    [InlineData("/api/v1/payments")]
    [InlineData("/api/v1/subscriptions")]
    [InlineData("/api/v1/attendances")]
    [InlineData("/api/v1/promo-codes")]
    [InlineData("/api/v1/notifications")]
    [InlineData("/api/v1/invoices/1")]
    [InlineData("/api/v1/coaches")]
    public async Task ProtectedEndpoints_WhenUnauthenticated_ReturnUnauthorized(string endpoint)
    {
        // Act
        var response = await _anonymousClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/dashboard/stats")]
    [InlineData("/api/v1/payments")]
    [InlineData("/api/v1/subscriptions")]
    [InlineData("/api/v1/attendances")]
    public async Task AdminOnlyEndpoints_WhenAccessedByMember_ReturnForbidden(string endpoint)
    {
        // Arrange
        var memberClient = _factory.CreateAuthenticatedClient("member-1", "member1@test.com", 1, nameof(Role.Member));

        // Act
        var response = await memberClient.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Subscriptions_WhenAccessedByCoach_ReturnsSuccess()
    {
        // Arrange
        var coachClient = _factory.CreateAuthenticatedClient("coach-1", "coach1@test.com", 1, nameof(Role.Coach));

        // Act
        var response = await coachClient.GetAsync("/api/v1/subscriptions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Notifications_WhenAuthenticated_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("user-10", "user10@test.com", 10, nameof(Role.Member));

        // Act
        var response = await client.GetAsync("/api/v1/notifications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
