using System.Security.Claims;
using Gym.Application.Features.Identity.Dtos;
using Gym.Domain.Attendance;
using Gym.Domain.Coaches;
using Gym.Domain.Identity;
using Gym.Domain.Members;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions;
using Gym.Domain.Subscriptions.Enums;
using Gym.Tests.Common.Reflection;
using Gym.Infrastructure.Identity;

namespace Gym.Application.SubcutaneousTests.Common;

public static class TestDataSeeder
{
    public static async Task<Member> AddMemberAsync(SubcutaneousTestContext context, string firstName = "Member", string lastName = "One", string email = "member1@gym.com")
    {
        var member = Member.Create(firstName, lastName, DateTime.UtcNow.AddYears(-25), "01000000000", "/images/member.jpg", DateTime.UtcNow.AddDays(-5), "note").Value;
        await context.DbContext.Members.AddAsync(member);
        await context.DbContext.SaveChangesAsync();
        context.IdentityService.SeedEmail(member.PersonId, email);
        return member;
    }

    public static async Task<Coach> AddCoachAsync(SubcutaneousTestContext context, string firstName = "Coach", string lastName = "One", string email = "coach1@gym.com")
    {
        var coach = Coach.Create(firstName, lastName, DateTime.UtcNow.AddYears(-30), "01000000001", "/images/coach.jpg", DateTime.UtcNow.AddDays(-30)).Value;
        await context.DbContext.Coaches.AddAsync(coach);
        await context.DbContext.SaveChangesAsync();
        context.IdentityService.SeedEmail(coach.PersonId, email);
        return coach;
    }

    public static async Task<Plan> AddPlanAsync(SubcutaneousTestContext context, string title = "Gold", decimal cost = 500m, int durationInDays = 30)
    {
        var plan = Plan.Create(title, "desc", cost, durationInDays, 2, 14).Value;
        await context.DbContext.Plans.AddAsync(plan);
        await context.DbContext.SaveChangesAsync();
        return plan;
    }

    public static async Task<Subscription> AddSubscriptionAsync(
        SubcutaneousTestContext context,
        Member member,
        Plan plan,
        DateOnly? startDate = null,
        SubscriptionStatus status = SubscriptionStatus.Pending)
    {
        var subscription = Subscription.Create(member.Id, plan, startDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).Value;

        switch (status)
        {
            case SubscriptionStatus.Active:
                subscription.Activate();
                break;
            case SubscriptionStatus.Scheduled:
                subscription.Scheduled();
                break;
            case SubscriptionStatus.Frozen:
                subscription.Activate();
                subscription.Freeze(0);
                break;
            case SubscriptionStatus.Cancelled:
                subscription.Cancel();
                break;
            case SubscriptionStatus.Expired:
                subscription.Activate();
                ReflectionTestHelper.SetProperty(subscription, "EndDate", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
                subscription.Expire();
                break;
        }

        await context.DbContext.Subscriptions.AddAsync(subscription);
        await context.DbContext.SaveChangesAsync();
        return subscription;
    }

    public static async Task<Payment> AddPaymentAsync(
        SubcutaneousTestContext context,
        Subscription subscription,
        bool markAsPaid = false,
        PaymentMethod paymentMethod = PaymentMethod.Cash)
    {
        var payment = Payment.Create(subscription).Value;
        await context.DbContext.Payments.AddAsync(payment);

        if (markAsPaid)
        {
            payment.Pay(paymentMethod);
        }

        await context.DbContext.SaveChangesAsync();
        return payment;
    }

    public static async Task<Attendance> AddAttendanceAsync(SubcutaneousTestContext context, Member member, DateTime? checkInAtUtc = null)
    {
        var attendance = Gym.Domain.Attendance.Attendance.Create(member.Id, checkInAtUtc ?? DateTime.UtcNow).Value;
        await context.DbContext.Attendances.AddAsync(attendance);
        await context.DbContext.SaveChangesAsync();
        return attendance;
    }

    public static async Task<RefreshToken> AddRefreshTokenAsync(SubcutaneousTestContext context, string userId, string token = "refresh-token")
    {
        if (!context.DbContext.Users.Any(u => u.Id == userId))
        {
            await context.DbContext.Users.AddAsync(new AppUser
            {
                Id = userId,
                UserName = userId,
                Email = $"{userId}@gym.com",
                NormalizedUserName = userId.ToUpperInvariant(),
                NormalizedEmail = $"{userId}@gym.com".ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString()
            });
            await context.DbContext.SaveChangesAsync();
        }

        var refreshToken = RefreshToken.Create(token, userId, DateTimeOffset.UtcNow.AddDays(1)).Value;
        await context.DbContext.RefreshTokens.AddAsync(refreshToken);
        await context.DbContext.SaveChangesAsync();
        return refreshToken;
    }

    public static AppUserDto CreateUser(string userId, int? personId, string email, string role)
    {
        return new AppUserDto(userId, personId, email, [role], [new Claim(ClaimTypes.NameIdentifier, userId)]);
    }
}
