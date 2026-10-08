namespace Gym.Application.Features.Dashboard.Dtos;
public sealed record AdminAttendanceSummaryResponse(
    int CurrentPeriodAttendance,
    int PreviousPeriodAttendance,
    decimal GrowthPercentage,
    decimal AverageDailyAttendance,
    int PeakHour,
    int PeakHourAttendanceCount,
    IReadOnlyList<AttendancePoint> Data);