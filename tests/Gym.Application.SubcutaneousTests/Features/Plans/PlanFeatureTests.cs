using Gym.Application.Common.Errors;
using Gym.Application.Features.Plans.Commands.CreatePlan;
using Gym.Application.Features.Plans.Commands.DeletePlan;
using Gym.Application.Features.Plans.Commands.UpdatePlan;
using Gym.Application.Features.Plans.Queries.GetPlanById;
using Gym.Application.Features.Plans.Queries.GetPlans;
using Gym.Application.SubcutaneousTests.Common;

namespace Gym.Application.SubcutaneousTests.Features.Plans;

public class PlanFeatureTests
{
    [Fact]
    public async Task CreatePlanCommand_ShouldCreatePlan()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new CreatePlanCommand("Premium", "desc", 900m, 60, 2, 10));

        Assert.True(result.IsSuccess);
        Assert.Single(context.DbContext.Plans);
    }

    [Fact]
    public async Task UpdatePlanCommand_ShouldUpdatePlan()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context);

        var result = await context.Mediator.Send(new UpdatePlanCommand(plan.Id, "Updated", "new", 1000m, 90, 3, 12));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", context.DbContext.Plans.First().Title);
    }

    [Fact]
    public async Task DeletePlanCommand_ShouldDeactivatePlan()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context);

        var result = await context.Mediator.Send(new DeletePlanCommand(plan.Id));

        Assert.True(result.IsSuccess);
        Assert.False(context.DbContext.Plans.First().IsActive);
    }

    [Fact]
    public async Task GetPlansQuery_ShouldFilterPlans()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        await TestDataSeeder.AddPlanAsync(context, "Gold");
        await TestDataSeeder.AddPlanAsync(context, "Silver");

        var result = await context.Mediator.Send(new GetPlansQuery(SearchTerm: "silver"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal("Silver", result.Value.Items!.First().Title);
    }

    [Fact]
    public async Task GetPlanByIdQuery_ShouldReturnNotFound_WhenMissing()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new GetPlanByIdQuery(999));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.PlanNotFound.Code, result.TopError.Code);
    }
}