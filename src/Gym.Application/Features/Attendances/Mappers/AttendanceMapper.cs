using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Attendance;

namespace Gym.Application.Features.Attendances.Mappers;

public static class AttendanceMapper
{
    public static AttendanceResponse ToDto(this Attendance attendance)
    {
        ArgumentNullException.ThrowIfNull(attendance);

        return new AttendanceResponse
        {
            AttendanceId = attendance.Id,
            MemberId = attendance.MemberId,
            MemberName = $"{attendance.Member.Person.FirstName} {attendance.Member.Person.LastName}",
            CheckInAtUtc = attendance.CheckInAtUtc
        };
    }
}