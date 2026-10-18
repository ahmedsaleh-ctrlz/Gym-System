using Gym.Application.Features.PromoCodes.Commands.ActivatePromoCode;
using Gym.Application.Features.PromoCodes.Commands.CreatePromoCodeCommand;
using Gym.Application.Features.PromoCodes.Commands.DeactivatePromoCode;
using Gym.Application.Features.PromoCodes.Queries.GetByCode;
using Gym.Application.Features.PromoCodes.Queries.GetPromoCodeByIdQuery;
using Gym.Application.Features.PromoCodes.Queries.GetPromoCodesQuery;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.PromoCodes.Enums;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.SubcutaneousTests.Features.PromoCodes;

public class PromoCodeFeatureTests
{
    [Fact]
    public async Task CreatePromoCodeCommand_ShouldPersistPromoCode_WhenValid()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context, "VIP Promo Plan");

        var command = new CreatePromoCodeCommand(
            "VIP2026",
            plan.Id,
            DiscountType.Percentage,
            25m,
            100m,
            200m,
            PromoCodeAudience.Everyone,
            20,
            DateTime.UtcNow.AddDays(30));

        var result = await context.Mediator.Send(command);

        Assert.True(result.IsSuccess);
        var created = await context.DbContext.PromoCodes.FirstOrDefaultAsync(p => p.Code == "VIP2026");
        Assert.NotNull(created);
        Assert.Equal(plan.Id, created.PlanId);
        Assert.Equal(25m, created.DiscountValue);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task DeactivateAndActivateCommands_ShouldToggleStatusInDatabase()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var promo = await TestDataSeeder.AddPromoCodeAsync(context, plan, "TOGGLE20");

        var deactResult = await context.Mediator.Send(new DeactivatePromoCodeCommand(promo.Id));
        Assert.True(deactResult.IsSuccess);
        Assert.False((await context.DbContext.PromoCodes.FindAsync(promo.Id))!.IsActive);

        var actResult = await context.Mediator.Send(new ActivatePromoCodeCommand(promo.Id));
        Assert.True(actResult.IsSuccess);
        Assert.True((await context.DbContext.PromoCodes.FindAsync(promo.Id))!.IsActive);
    }

    [Fact]
    public async Task GetPromoCodeByIdQuery_ShouldReturnDetails_WhenFound()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context, "Plan For Promo");
        var promo = await TestDataSeeder.AddPromoCodeAsync(context, plan, "PROMO100");

        var result = await context.Mediator.Send(new GetPromoCodeByIdQuery(promo.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(promo.Id, result.Value.Id);
        Assert.Equal("PROMO100", result.Value.Code);
        Assert.Equal("Plan For Promo", result.Value.PlanName);
    }

    [Fact]
    public async Task GetPromoCodesQuery_ShouldReturnListOfPromoCodes()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddPromoCodeAsync(context, plan, "PROMO1");
        await TestDataSeeder.AddPromoCodeAsync(context, plan, "PROMO2");

        var result = await context.Mediator.Send(new GetPromoCodesQuery());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Count() >= 2);
    }
}
