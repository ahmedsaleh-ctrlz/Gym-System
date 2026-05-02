namespace Gym.Api.Contracts.Members;

public sealed record UpdateMemberRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string PhoneNumber,
    DateTime JoinDate,
    string? Notes);
