using System.Net;

namespace Gym.Api.IntegrationTests;

public sealed class HealthCheckTests : IClassFixture<GymWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(GymWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ReturnsSuccessAndExpectedMessage()
    {
        // Act
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Failed with status {response.StatusCode} and body: {content}");
        Assert.Equal("Api Is Running", content);
    }
}
