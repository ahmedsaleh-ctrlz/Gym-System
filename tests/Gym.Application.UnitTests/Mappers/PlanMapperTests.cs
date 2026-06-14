using Gym.Application.Features.Plans.Mappers;
using Gym.Tests.Common.Plans;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class PlanMapperTests
{
    [Fact]
    public void ToDetailsDto_ShouldThrowArgumentNullException_WhenPlanIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => PlanMapper.ToDetailsDto(null!));
    }

    [Fact]
    public void ToDetailsDto_ShouldMapPlanToResponse()
    {
        var plan = PlanFactory.CreatePlan(title: "Gold", description: "Desc", cost: 800m, durationInDays: 45, allowedFreezeCount: 2, maxTotalFreezeDays: 8).Value;
        ReflectionTestHelper.SetProperty(plan, "Id", 4);

        var result = plan.ToDetailsDto();

        Assert.Equal(4, result.PlanId);
        Assert.Equal("Gold", result.Title);
        Assert.Equal("Desc", result.Description);
        Assert.Equal(800m, result.Cost);
        Assert.Equal(45, result.DurationInDays);
        Assert.True(result.IsActive);
        Assert.Equal(2, result.AllowedFreezeCount);
        Assert.Equal(8, result.MaxTotalFreezeDays);
    }
}
