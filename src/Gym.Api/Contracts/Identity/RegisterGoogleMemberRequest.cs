using System.ComponentModel.DataAnnotations;

namespace Gym.Api.Contracts.Identity;

public sealed record RegisterGoogleMemberRequest
{
    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; init; }

    [Required]
    public string PhoneNumber { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }
}