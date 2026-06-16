using Gym.Application.Features.Attendances.Mappers;
using Gym.Domain.Attendance;
using Gym.Tests.Common.Attendance;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class AttendanceMapperTests
{
    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenAttendanceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => AttendanceMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_ShouldMapAttendanceToResponse()
    {
        var member = MemberFactory.CreateMember(firstName: "Mona", lastName: "Ali").Value;
        ReflectionTestHelper.SetProperty(member, "Id", 7);

        var attendance = AttendanceFactory.CreateAttendance(memberId: 7, checkInAtUtc: DateTime.UtcNow).Value;
        ReflectionTestHelper.SetProperty(attendance, "Id", 11);
        ReflectionTestHelper.SetProperty(attendance, "Member", member);

        var result = attendance.ToDto();

        Assert.Equal(11, result.AttendanceId);
        Assert.Equal(7, result.MemberId);
        Assert.Equal("Mona Ali", result.MemberName);
        Assert.Equal(attendance.CheckInAtUtc, result.CheckInAtUtc);
    }
}