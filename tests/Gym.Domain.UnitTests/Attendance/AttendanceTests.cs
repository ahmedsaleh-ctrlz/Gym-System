using Gym.Domain.Attendance;
using Gym.Tests.Common.Attendance;

namespace Gym.Domain.UnitTests.Attendance;

public class AttendanceTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenMemberIdIsInvalid()
    {
        var result = AttendanceFactory.CreateAttendance(memberId: 0);

        Assert.False(result.IsSuccess);
        Assert.Equal(AttendanceErrors.InvalidMemberId.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenCheckInTimeIsInFuture()
    {
        var result = AttendanceFactory.CreateAttendance(checkInAtUtc: DateTime.UtcNow.AddMinutes(2));

        Assert.False(result.IsSuccess);
        Assert.Equal(AttendanceErrors.InvalidCheckInTime.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var checkInAtUtc = DateTime.UtcNow;

        var result = AttendanceFactory.CreateAttendance(memberId: 5, checkInAtUtc: checkInAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.MemberId);
        Assert.Equal(DateTimeKind.Utc, result.Value.CheckInAtUtc.Kind);
    }

    [Fact]
    public void Create_ShouldAllowCheckInWithinOneMinuteTolerance()
    {
        var result = AttendanceFactory.CreateAttendance(checkInAtUtc: DateTime.UtcNow.AddSeconds(30));

        Assert.True(result.IsSuccess);
    }
}
