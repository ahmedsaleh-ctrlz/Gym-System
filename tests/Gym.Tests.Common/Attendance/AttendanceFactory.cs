using Gym.Domain.Attendance;
using Gym.Domain.Common.Result;

namespace Gym.Tests.Common.Attendance;

public static class AttendanceFactory
{
    public static Result<Gym.Domain.Attendance.Attendance> CreateAttendance(
        int? memberId = null,
        DateTime? checkInAtUtc = null)
    {
        return Gym.Domain.Attendance.Attendance.Create(
            memberId ?? 1,
            checkInAtUtc ?? DateTime.UtcNow);
    }
}
