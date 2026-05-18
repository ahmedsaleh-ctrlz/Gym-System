using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;

namespace Gym.Domain.Attendance;

public sealed class Attendance : AuditableEntity
{
    public int MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public DateTime CheckInAtUtc { get; private set; }

    private Attendance()
    {
    }

    private Attendance(int memberId, DateTime checkInAtUtc)
    {
        MemberId = memberId;
        CheckInAtUtc = DateTime.SpecifyKind(checkInAtUtc, DateTimeKind.Utc);
    }

    public static Result<Attendance> Create(int memberId, DateTime checkInAtUtc)
    {
        var error = Validate(memberId, checkInAtUtc);
        if (error is not null)
        {
            return error;
        }

        return new Attendance(memberId, checkInAtUtc);
    }

    private static Error? Validate(int memberId, DateTime checkInAtUtc)
    {
        if (memberId <= 0)
        {
            return AttendanceErrors.InvalidMemberId;
        }

        if (checkInAtUtc > DateTime.UtcNow.AddMinutes(1))
        {
            return AttendanceErrors.InvalidCheckInTime;
        }

        return null;
    }
}
