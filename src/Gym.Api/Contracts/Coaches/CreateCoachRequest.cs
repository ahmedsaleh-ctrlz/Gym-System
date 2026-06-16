namespace Gym.Api.Contracts.Coaches;

public sealed record CreateCoachRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    string ImageUrl,
    DateTime HireDate,
    string Email,
    string Password);