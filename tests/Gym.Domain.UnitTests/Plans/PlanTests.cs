using Gym.Domain.Plans;
using Gym.Tests.Common.Plans;

namespace Gym.Domain.UnitTests.Plans;

public class PlanTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenTitleIsMissing()
    {
        var result = PlanFactory.CreatePlan(title: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.TitleRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenCostIsNegative()
    {
        var result = PlanFactory.CreatePlan(cost: -1m);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidCost.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenDurationIsNegative()
    {
        var result = PlanFactory.CreatePlan(durationInDays: -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvaildDuration.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenAllowedFreezeCountIsNegative()
    {
        var result = PlanFactory.CreatePlan(allowedFreezeCount: -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidAllowedFreezeCount.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenMaxTotalFreezeDaysIsNegative()
    {
        var result = PlanFactory.CreatePlan(maxTotalFreezeDays: -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidMaxTotalFreezeDays.Code, result.TopError.Code);
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
    public void UpdateInfo_ShouldReturnError_WhenTitleIsMissing()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo(string.Empty, "Desc", 500m, 30, 2, 14);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.TitleRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenCostIsNegative()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo("Title", "Desc", -1m, 30, 2, 14);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidCost.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenDurationIsNegative()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo("Title", "Desc", 500m, -1, 2, 14);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvaildDuration.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenAllowedFreezeCountIsNegative()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo("Title", "Desc", 500m, 30, -1, 14);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidAllowedFreezeCount.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenMaxTotalFreezeDaysIsNegative()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo("Title", "Desc", 500m, 30, 2, -1);

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.InvalidMaxTotalFreezeDays.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnSuccess_WhenDataIsValid()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.UpdateInfo("Premium", "New Desc", 900m, 90, 3, 21);

        Assert.True(result.IsSuccess);
        Assert.Equal("Premium", plan.Title);
        Assert.Equal("New Desc", plan.Description);
        Assert.Equal(900m, plan.Cost);
        Assert.Equal(90, plan.DurationInDays);
        Assert.Equal(3, plan.AllowedFreezeCount);
        Assert.Equal(21, plan.MaxTotalFreezeDays);
    }

    [Fact]
    public void Activate_ShouldReturnError_WhenPlanAlreadyActive()
    {
        var plan = PlanFactory.CreatePlan().Value;

        var result = plan.Activate();

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.PlanAlreadyActive.Code, result.TopError.Code);
    }

    [Fact]
    public void Deactivate_ShouldReturnError_WhenPlanAlreadyInactive()
    {
        var plan = PlanFactory.CreatePlan().Value;
        plan.Deactivate();

        var result = plan.Deactivate();

        Assert.False(result.IsSuccess);
        Assert.Equal(PlanError.PlanAlreadyInactive.Code, result.TopError.Code);
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