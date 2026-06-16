using Gym.Application.Common.Errors;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Application.Features.Payments.Queries.GetMemberPayments;
using Gym.Application.Features.Payments.Queries.GetPaymentById;
using Gym.Application.Features.Payments.Queries.GetPayments;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Application.SubcutaneousTests.Features.Payments;

public class PaymentFeatureTests
{
    [Fact]
    public async Task PayPaymentCommand_ShouldActivateTodaySubscription_WhenPaymentSucceeds()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        var payment = await TestDataSeeder.AddPaymentAsync(context, subscription);

        var result = await context.Mediator.Send(new PayPaymentCommand(payment.Id, PaymentMethod.Cash));

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Paid, context.DbContext.Payments.First().Status);
        Assert.Equal(SubscriptionStatus.Active, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task PayPaymentCommand_ShouldScheduleFutureSubscription_WhenPaymentSucceeds()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)));
        var payment = await TestDataSeeder.AddPaymentAsync(context, subscription);

        var result = await context.Mediator.Send(new PayPaymentCommand(payment.Id, PaymentMethod.Visa));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Scheduled, context.DbContext.Subscriptions.First().Status);
    }

    [Fact]
    public async Task GetPaymentsQuery_ShouldFilterByMember()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var memberOne = await TestDataSeeder.AddMemberAsync(context, "One", "Member");
        var memberTwo = await TestDataSeeder.AddMemberAsync(context, "Two", "Member");
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subOne = await TestDataSeeder.AddSubscriptionAsync(context, memberOne, plan);
        var subTwo = await TestDataSeeder.AddSubscriptionAsync(context, memberTwo, plan, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        await TestDataSeeder.AddPaymentAsync(context, subOne, true);
        await TestDataSeeder.AddPaymentAsync(context, subTwo, true);

        var result = await context.Mediator.Send(new GetPaymentsQuery(MemberId: memberOne.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal(memberOne.Id, result.Value.Items!.First().MemberId);
    }

    [Fact]
    public async Task GetPaymentByIdQuery_ShouldReturnNotFound_WhenMissing()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new GetPaymentByIdQuery(404));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.PaymentNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task GetMemberPaymentsQuery_ShouldReturnMemberPayments()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        var subscription = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        await TestDataSeeder.AddPaymentAsync(context, subscription, true);

        var result = await context.Mediator.Send(new GetMemberPaymentsQuery(member.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(member.Id, result.Value[0].MemberId);
    }
}