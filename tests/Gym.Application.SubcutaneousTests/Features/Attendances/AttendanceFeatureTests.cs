using Gym.Application.Common.Errors;
using Gym.Application.Features.Attendances.Commands.CheckInMember;
using Gym.Application.Features.Attendances.Queries.GetAttendances;
using Gym.Application.Features.Attendances.Queries.GetMemberAttendanceHistory;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Application.SubcutaneousTests.Features.Attendances;

public class AttendanceFeatureTests
{
    [Fact]
    public async Task CheckInMemberCommand_ShouldCreateAttendance_ForActiveSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context, "Check", "In");
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);

        var result = await context.Mediator.Send(new CheckInMemberCommand(member.Id));

        Assert.True(result.IsSuccess);
        Assert.Single(context.DbContext.Attendances);
        Assert.Equal(member.Id, result.Value.MemberId);
    }

    [Fact]
    public async Task CheckInMemberCommand_ShouldFail_OnSecondCheckInSameDay()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);
        await TestDataSeeder.AddAttendanceAsync(context, member, DateTime.UtcNow);

        var result = await context.Mediator.Send(new CheckInMemberCommand(member.Id));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvalidCheckInTime.Code, result.TopError.Code);
    }

    [Fact]
    public async Task GetAttendancesQuery_ShouldFilterAttendances()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var memberOne = await TestDataSeeder.AddMemberAsync(context, "Alpha", "One");
        var memberTwo = await TestDataSeeder.AddMemberAsync(context, "Beta", "Two");
        await TestDataSeeder.AddAttendanceAsync(context, memberOne);
        await TestDataSeeder.AddAttendanceAsync(context, memberTwo, DateTime.UtcNow.AddMinutes(-10));

        var result = await context.Mediator.Send(new GetAttendancesQuery(SearchTerm: "beta"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal(memberTwo.Id, result.Value.Items!.First().MemberId);
    }

    [Fact]
    public async Task GetMemberAttendanceHistoryQuery_ShouldReturnNotFound_WhenMemberMissing()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new GetMemberAttendanceHistoryQuery(999));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.MemberNotFound.Code, result.TopError.Code);
    }
}
