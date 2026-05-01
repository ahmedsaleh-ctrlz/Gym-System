namespace Gym.Application.Features.Members.Dtos;

public sealed record MemberResponse
{
    public int MemberId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime JoinDate { get; set; }
    public string? Notes { get; set; }

}
