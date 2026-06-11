using Gym.Domain.Plans;
using Gym.Tests.Common.Plans;

namespace Gym.Domain.UnitTests.Plans;

public class PlanTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenTitleIsMissing()
    {
        var result = PlanFactory.CreatePlan(title: "");

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.TitleRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = PlanFactory.CreatePlan(cost: 750m, durationInDays: 60, allowedFreezeCount: 1, maxTotalFreezeDays: 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(750m, result.Value.Cost);
        Assert.Equal(60, result.Value.DurationInDays);
        Assert.Equal(1, result.Value.AllowedFreezeCount);
        Assert.Equal(10, result.Value.MaxTotalFreezeDays);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenPlanIsInactive()
    {
        var plan = PlanFactory.CreatePlan().Value;
        plan.Deactivate();

        var result = plan.UpdateInfo("New", "Desc", 500m, 30, 2, 14);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.CannotUpdateInactivePlan.Code, result.TopError.Code);
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldUpdatePlanState()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var deactivateResult = plan.Deactivate();
        var activateResult = plan.Activate();

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.True(plan.IsActive);
    }
}
