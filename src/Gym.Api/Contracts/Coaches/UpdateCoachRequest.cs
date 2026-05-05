namespace Gym.Api.Contracts.Coaches;

public sealed record UpdateCoachRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    DateTime HireDate);
