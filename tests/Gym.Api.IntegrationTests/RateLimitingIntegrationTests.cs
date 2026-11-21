using System.Net;
using System.Net.Http.Json;
using Gym.Api.Contracts.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.IntegrationTests;

public sealed class RateLimitingIntegrationTests : IClassFixture<GymWebApplicationFactory>, IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(GymWebApplicationFactory factory)
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
    public async Task RootEndpoint_WithDisableRateLimiting_NeverReturns429()
    {
        // Act: Make 15 consecutive requests to GET /
        for (var i = 0; i < 15; i++)
        {
            var response = await _client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task HealthEndpoint_WithDisableRateLimiting_NeverReturns429()
    {
        // Act: Make 15 consecutive requests to GET /health
        for (var i = 0; i < 15; i++)
        {
            var response = await _client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task LoginRateLimiter_WhenExceeding5RequestsPerMinute_Returns429WithProblemDetails()
    {
        var request = new GenerateTokenRequest("ratelimit-test@gym.com", "WrongPassword123!");
        HttpResponseMessage? lastResponse = null;

        // Act: Send 6 requests in rapid succession (Limit is 5 per minute)
        for (var i = 0; i < 6; i++)
        {
            lastResponse = await _client.PostAsJsonAsync("/api/v1/identity/token/generate", request);
        }

        // Assert: 6th request must be throttled
        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);

        // Verify ProblemDetails JSON response format
        var problemDetails = await lastResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problemDetails.Status);
        Assert.Equal("Too Many Requests", problemDetails.Title);
        Assert.Contains("exceeded", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.True(problemDetails.Extensions.ContainsKey("requestId"));
    }

    [Fact]
    public async Task ResendEmail_WhenCalledMoreThanOncePerMinute_Returns429()
    {
        var request = new ForgetPasswordRequest("email-limit-test@gym.com");

        // First call should pass through rate limiter (may return NotFound/Created based on email)
        var firstResponse = await _client.PostAsJsonAsync("/api/v1/identity/forget-password", request);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, firstResponse.StatusCode);

        // Second immediate call should be throttled by the 1-token-per-minute bucket
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/identity/forget-password", request);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);

        // Verify ProblemDetails format
        var problemDetails = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problemDetails.Status);
    }
}
