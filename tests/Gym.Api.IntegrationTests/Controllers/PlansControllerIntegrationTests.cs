using System.Net;
using System.Net.Http.Json;
using Gym.Api.Contracts.Plans;
using Gym.Application.Common.Models;
using Gym.Application.Features.Plans.Dtos;
using Gym.Domain.Identity;

namespace Gym.Api.IntegrationTests.Controllers;

public sealed class PlansControllerIntegrationTests : IClassFixture<GymWebApplicationFactory>, IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;

    public PlansControllerIntegrationTests(GymWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPlans_AnonymousUser_ReturnsOkAndPagedList()
    {
        // Act
        var response = await _anonymousClient.GetAsync("/api/v1/plans?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<PlanDetailsResponse>>();
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetPlanById_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Act
        var response = await _anonymousClient.GetAsync("/api/v1/plans/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanById_AuthenticatedUser_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("user-1", "user1@example.com", 1, nameof(Role.Member));

        // Act
        var response = await client.GetAsync("/api/v1/plans/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlan_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreatePlanRequest("Gold Plan", "Description", 100m, 30, 2, 10);

        // Act
        var response = await _anonymousClient.PostAsJsonAsync("/api/v1/plans", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlan_MemberUser_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("member-1", "member1@example.com", 1, nameof(Role.Member));
        var request = new CreatePlanRequest("Gold Plan", "Description", 100m, 30, 2, 10);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/plans", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlan_AdminUser_WhenInvalidPayload_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("admin-1", "admin@example.com", 1, nameof(Role.Admin));

        // Negative cost and empty title
        var invalidRequest = new CreatePlanRequest(string.Empty, "Description", -10m, -5, -1, -1);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/plans", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlan_AdminUser_WhenValidPayload_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient("admin-1", "admin@example.com", 1, nameof(Role.Admin));
        var uniqueTitle = $"Premium Plan {Guid.NewGuid():N}";
        var request = new CreatePlanRequest(uniqueTitle, "All inclusive premium gym pass", 150m, 30, 2, 10);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/plans", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
