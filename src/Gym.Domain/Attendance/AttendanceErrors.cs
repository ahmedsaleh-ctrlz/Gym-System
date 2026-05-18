using Gym.Domain.Common.Result;

namespace Gym.Domain.Attendance;

public static class AttendanceErrors
{
    public static Error InvalidMemberId =>
        Error.Validation("Attendance.InvalidMemberId", "Attendance must be linked to a valid member.");

    public static Error InvalidCheckInTime =>
        Error.Validation("Attendance.InvalidCheckInTime", "Attendance timestamp cannot be in the future.");

}
