namespace Gym.Application.Features.Members.Dtos;

public sealed record ActiveMemberResponse
{
    public int MemberId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
}