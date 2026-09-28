namespace Gym.Api.Contracts.Identity;

public sealed record RegisterMemberRequest(string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    string Email,
    string Password,
    string? ImageUrl);