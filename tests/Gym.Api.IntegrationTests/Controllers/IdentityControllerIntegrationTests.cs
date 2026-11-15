using System.Net;
using System.Net.Http.Json;
using Gym.Api.Contracts.Identity;

namespace Gym.Api.IntegrationTests.Controllers;

public sealed class IdentityControllerIntegrationTests : IClassFixture<GymWebApplicationFactory>, IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IdentityControllerIntegrationTests(GymWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GenerateToken_WithNonexistentEmail_ReturnsNotFound()
    {
        // Arrange
        var request = new GenerateTokenRequest("nonexistent@gym.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/identity/token/generate", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithFutureDateOfBirth_ReturnsBadRequest()
    {
        // Arrange - future birth date violates domain invariant
        var request = new RegisterMemberRequest(
            FirstName: "Test",
            LastName: "User",
            DateOfBirth: DateTime.UtcNow.AddYears(5),
            PhoneNumber: "1234567890",
            Email: "futurebirth@example.com",
            Password: "Password123!",
            ImageUrl: "http://example.com/image.png");

        // Act - V2 endpoint
        var response = await _client.PostAsJsonAsync("/api/v2/identity", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterMember_WithValidPayload_ReturnsCreated()
    {
        // Arrange
        var uniqueEmail = $"validuser_{Guid.NewGuid():N}@example.com";
        var request = new RegisterMemberRequest(
            FirstName: "Valid",
            LastName: "Member",
            DateOfBirth: DateTime.UtcNow.AddYears(-25),
            PhoneNumber: "1234567890",
            Email: uniqueEmail,
            Password: "Password123!",
            ImageUrl: "http://example.com/image.png");

        // Act - V2 endpoint
        var response = await _client.PostAsJsonAsync("/api/v2/identity", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
