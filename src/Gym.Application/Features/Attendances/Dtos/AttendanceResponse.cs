namespace Gym.Application.Features.Attendances.Dtos;

public sealed record AttendanceResponse
{
    public int AttendanceId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime CheckInAtUtc { get; set; }
}