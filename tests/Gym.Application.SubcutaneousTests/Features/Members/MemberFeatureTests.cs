using Gym.Application.Common.Errors;
using Gym.Application.Features.Members.Commands.CreateMember;
using Gym.Application.Features.Members.Commands.DeleteMember;
using Gym.Application.Features.Members.Commands.UpdateMember;
using Gym.Application.Features.Members.Queries.GetCurrentMember;
using Gym.Application.Features.Members.Queries.GetMemberById;
using Gym.Application.Features.Members.Queries.GetMembers;
using Gym.Application.Features.Members.Queries.GetMembersWithActiveSubscription;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Application.SubcutaneousTests.Features.Members;

public class MemberFeatureTests
{
    [Fact]
    public async Task CreateMemberCommand_ShouldCreateMember_AndUser()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var command = new CreateMemberCommand("Mona", "Ali", DateTime.UtcNow.AddYears(-22), "01010000000", "/images/mona.jpg", DateTime.UtcNow.AddDays(-1), "vip", "mona@gym.com", "123456");

        var result = await context.Mediator.Send(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("Mona", result.Value.FirstName);
        Assert.Single(context.DbContext.Members);
    }

    [Fact]
    public async Task UpdateMemberCommand_ShouldUpdatePersistedMember()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context, "Old", "Name");

        var result = await context.Mediator.Send(new UpdateMemberCommand(member.Id, "New", "Member", DateTime.UtcNow.AddYears(-23), "01111111111", DateTime.UtcNow.AddDays(-2), "updated"));

        Assert.True(result.IsSuccess);
        Assert.Equal("New", context.DbContext.Members.First().Person.FirstName);
    }

    [Fact]
    public async Task UpdateMemberImageCommand_ShouldUpdateImage()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);

        var result = await context.Mediator.Send(new UpdateMemberImageCommand(member.Id, "/images/updated-member.jpg"));

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/updated-member.jpg", context.DbContext.Members.First().Person.Image.ImageUrl);
    }

    [Fact]
    public async Task DeleteMemberCommand_ShouldFail_WhenMemberHasCurrentSubscription()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context);
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, member, plan, status: SubscriptionStatus.Active);

        var result = await context.Mediator.Send(new DeleteMemberCommand(member.Id));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.CannotDeleteSubscribedMember.Code, result.TopError.Code);
    }

    [Fact]
    public async Task GetMembersQuery_ShouldReturnFilteredMembers()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        await TestDataSeeder.AddMemberAsync(context, "Alpha", "One");
        await TestDataSeeder.AddMemberAsync(context, "Beta", "Two");

        var result = await context.Mediator.Send(new GetMembersQuery(SearchTerm: "beta"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal("Beta", result.Value.Items!.First().FirstName);
    }

    [Fact]
    public async Task GetMemberByIdQuery_ShouldReturnMemberWithEmail()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context, email: "member@get.com");

        var result = await context.Mediator.Send(new GetMemberByIdQuery(member.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("member@get.com", result.Value.Email);
    }

    [Fact]
    public async Task GetCurrentMemberQuery_ShouldReturnCurrentMember()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context, email: "current@get.com");
        context.CurrentUser.PersonId = member.PersonId.ToString();

        var result = await context.Mediator.Send(new GetCurrentMemberQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(member.Id, result.Value.MemberId);
        Assert.Equal("current@get.com", result.Value.Email);
    }

    [Fact]
    public async Task GetMembersWithActiveSubscriptionQuery_ShouldReturnOnlyActiveMembers()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var activeMember = await TestDataSeeder.AddMemberAsync(context, "Active", "Member");
        var inactiveMember = await TestDataSeeder.AddMemberAsync(context, "Inactive", "Member");
        var plan = await TestDataSeeder.AddPlanAsync(context);
        await TestDataSeeder.AddSubscriptionAsync(context, activeMember, plan, status: SubscriptionStatus.Active);
        await TestDataSeeder.AddSubscriptionAsync(context, inactiveMember, plan, status: SubscriptionStatus.Pending);

        var result = await context.Mediator.Send(new GetMembersWithActiveSubscriptionQuery());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(activeMember.Id, result.Value[0].MemberId);
    }
}
