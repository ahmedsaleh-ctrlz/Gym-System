namespace Gym.Api.Contracts.Identity;

public sealed record GenerateTokenRequest(string Email,
    string Password);