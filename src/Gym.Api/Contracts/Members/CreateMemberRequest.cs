namespace Gym.Api.Contracts.Members;

public sealed record CreateMemberRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    string ImageUrl,
    DateTime JoinDate,
    string? Notes,
    string Email,
    string Password);