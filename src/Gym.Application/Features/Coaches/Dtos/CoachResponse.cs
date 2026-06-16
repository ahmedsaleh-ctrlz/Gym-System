namespace Gym.Application.Features.Coaches.Dtos;

public sealed record CoachResponse
{
    public int CoachId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime HireDate { get; set; }
}