namespace Gym.Api.Contracts.Identity;

public sealed record GenerateTokenRequest(string email,
    string password);

