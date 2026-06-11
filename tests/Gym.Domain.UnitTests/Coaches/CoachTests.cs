using Gym.Domain.Coaches;
using Gym.Tests.Common.Coaches;

namespace Gym.Domain.UnitTests.Coaches;

public class CoachTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenHireDateIsInFuture()
    {
        var result = CoachFactory.CreateCoach(hireDate: DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.InvalidHireDate.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = CoachFactory.CreateCoach();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.NotNull(result.Value.Person);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenCoachIsInactive()
    {
        var coach = CoachFactory.CreateCoach().Value;
        coach.Deactivate();

        var result = coach.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-20), "010", DateTime.UtcNow.AddDays(-3));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.CannotUpdateInactiveCoach.Code, result.TopError.Code);
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldUpdateCoachState()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var deactivateResult = coach.Deactivate();
        var activateResult = coach.Activate();

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.True(coach.IsActive);
    }
}
