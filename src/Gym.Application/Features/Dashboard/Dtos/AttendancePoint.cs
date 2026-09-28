namespace Gym.Application.Features.Dashboard.Dtos
{
    public sealed record AttendancePoint
    {
        public DateOnly Date { get; init; }
        public int Count { get; init; }
    }
}