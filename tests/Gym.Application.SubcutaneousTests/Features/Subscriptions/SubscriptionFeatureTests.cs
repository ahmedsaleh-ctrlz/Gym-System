using Gym.Application.Common.Errors;
using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ActivateScheduledSubscriptions;
using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.ExpireSubscriptions;
using Gym.Application.Features.Subscriptions.Commands.BackgroundJobs.UnfreezeSubscriptions;
using Gym.Application.Features.Subscriptions.Commands.CreateSubscription;
using Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;
using Gym.Application.Features.Subscriptions.Commands.RenewSubscription;
using Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;
using Gym.Application.Features.Subscriptions.Queries.GetMemberSubscriptions;
using Gym.Application.Features.Subscriptions.Queries.GetSubscriptionById;
using Gym.Application.Features.Subscriptions.Queries.GetSubscriptions;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions.Enums;
using Gym.Tests.Common.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.SubcutaneousTests.Features.Subscriptions;

public class SubscriptionFeatureTests
{
    [Fact]
    public async Task CreateSubscriptionCommand_ShouldCreateSubscription_AndPayment()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);

        var result = await context.Mediator.Send(new CreateSubscriptionCommand(member.Id, plan.Id, DateOnly.FromDateTime(DateTime.UtcNow)));

        Assert.True(result.IsSuccess);
        Assert.Single(context.DbContext.Subscriptions);
        Assert.Single(context.DbContext.Payments);
    }

    [Fact]
    public async Task CreateSubscriptionCommand_ShouldFail_WhenMemberHasActiveSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);

        var result = await context.Mediator.Send(new CreateSubscriptionCommand(member.Id, plan.Id, DateOnly.FromDateTime(DateTime.UtcNow)));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.CannotCreateSubscriptionForMemberWithActiveOrFrozenSubscription.Code, result.TopError.Code);
    }

    [Fact]
    public async Task UpdateSubscriptionStatusCommand_ShouldSchedulePendingSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);

        var result = await context.Mediator.Send(new UpdateSubscriptionStatusCommand(subscription.Id, SubscriptionStatus.Scheduled));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Scheduled, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task FreezeSubscriptionCommand_ShouldFreezeActiveSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);

        var result = await context.Mediator.Send(new FreezeSubscriptionCommand(subscription.Id, 3));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Frozen, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task RenewSubscriptionCommand_ShouldCreateNewPendingSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var existing = await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);
        ReflectionTestHelper.SetProperty(existing, "EndDate", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)));
        await context.DbContext.SaveChangesAsync();

        var result = await context.Mediator.Send(new RenewSubscriptionCommand(member.Id, plan.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, context.DbContext.Subscriptions.IgnoreQueryFilters().Count());
        Assert.Equal(2, context.DbContext.Subscriptions.Count());
        Assert.Single(context.DbContext.Payments);
    }

    [Fact]
    public async Task ActivateScheduledSubscriptionsCommand_ShouldActivateDueSubscriptions()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow), SubscriptionStatus.Scheduled);

        await context.Mediator.Send(new ActivateScheduledSubscriptionsCommand());

        Assert.Equal(SubscriptionStatus.Active, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task ExpireSubscriptionsCommand_ShouldExpirePastActiveSubscriptions()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);
        ReflectionTestHelper.SetProperty(subscription, "EndDate", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        await context.DbContext.SaveChangesAsync();

        await context.Mediator.Send(new ExpireSubscriptionsCommand());

        Assert.Equal(SubscriptionStatus.Expired, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task UnfreezeSubscriptionsCommand_ShouldUnfreezeEligibleSubscriptions()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Frozen);

        await context.Mediator.Send(new UnfreezeSubscriptionsCommand());

        Assert.Equal(SubscriptionStatus.Active, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task GetSubscriptionsQuery_ShouldFilterByPlanName()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var gold = await TestDataSeeder.AddPlanAsync(context, "Gold");
        var silver = await TestDataSeeder.AddPlanAsync(context, "Silver");
        await TestDataSeeder.AddSubscriptionAsync(context, member, gold);
        await TestDataSeeder.AddSubscriptionAsync(context, member, silver, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await context.Mediator.Send(new GetSubscriptionsQuery(PlanName: "silver"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal("Silver", result.Value.Items!.First().PlanName);
    }

    [Fact]
    public async Task GetSubscriptionByMemberIdQuery_ShouldReturnLatestSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow));
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await context.Mediator.Send(new GetSubscriptionByMemberIdQuery(member.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(member.Id, result.Value.MemberId);
    }

    [Fact]
    public async Task GetMemberSubscriptionsQuery_ShouldReturnDescendingSubscriptions()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow));
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)));

        var result = await context.Mediator.Send(new GetMemberSubscriptionsQuery(member.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.True(result.Value[0].EndDate >= result.Value[1].EndDate);
    }
}
