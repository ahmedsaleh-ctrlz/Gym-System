using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.Overview;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard.PaymentsSummary;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Payments.Enums;

namespace Gym.Application.SubcutaneousTests.Features.Dashboard;

public class DashboardFeatureTests
{
    [Fact]
    public async Task GetAdminDashboardOverviewQuery_ShouldReturnAggregatedMetrics()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context, "Gold");
        var active = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        await TestDataSeeder.AddAttendanceAsync(context, member, DateTime.UtcNow);
        var payment = await TestDataSeeder.AddPaymentAsync(context, active);
        await context.Mediator.Send(new PayPaymentCommand(payment.Id, PaymentMethod.Cash));

        var result = await context.Mediator.Send(new GetAdminDashboardOverviewQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalMembers);
        Assert.Equal(1, result.Value.ActiveSubscriptions);
        Assert.Equal(1, result.Value.TodayAttendanceCount);
        Assert.Equal("Gold", result.Value.MostPopularPlan);
        Assert.Equal(1, result.Value.MostPopularPlanSubscriptionsCount);
        Assert.Equal(500m, result.Value.ThisMonthRevenue);
    }

    [Fact]
    public async Task GetAdminPaymentsSummaryQuery_ShouldReturnPaymentCountsAndTotals()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context, "Platinum", 1000m);
        var active = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        var payment = await TestDataSeeder.AddPaymentAsync(context, active);
        await context.Mediator.Send(new PayPaymentCommand(payment.Id, PaymentMethod.Visa));

        var result = await context.Mediator.Send(new GetAdminPaymentsSummaryQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PaidPaymentsCount);
        Assert.Equal(0, result.Value.PendingPaymentsCount);
        Assert.Equal(0, result.Value.UnderReviewPaymentsCount);
    }
}