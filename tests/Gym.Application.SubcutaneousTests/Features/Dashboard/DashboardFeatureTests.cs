using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Payments.Enums;

namespace Gym.Application.SubcutaneousTests.Features.Dashboard;

public class DashboardFeatureTests
{
    [Fact]
    public async Task GetAdminDashboardQuery_ShouldReturnAggregatedMetrics()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context, "Gold");
        var active = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        await TestDataSeeder.AddAttendanceAsync(context, member, DateTime.UtcNow);
        var payment = await TestDataSeeder.AddPaymentAsync(context, active);
        await context.Mediator.Send(new PayPaymentCommand(payment.Id, PaymentMethod.Cash));

        var result = await context.Mediator.Send(new GetAdminDashboardQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalMembers);
        Assert.Equal(1, result.Value.ActiveSubscriptions);
        Assert.Equal(1, result.Value.TodayAttendanceCount);
        Assert.Equal("Gold", result.Value.MostPopularPlan);
        Assert.Equal(1, result.Value.PaidPaymentsCount);
    }
}