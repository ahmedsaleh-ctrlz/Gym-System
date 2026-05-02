namespace Gym.Client.Models;

public sealed class MemberItem
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
