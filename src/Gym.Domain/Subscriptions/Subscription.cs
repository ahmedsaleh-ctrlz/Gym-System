using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions.Enums;

namespace Gym.Domain.Subscriptions;

public sealed class Subscription : AuditableEntity
{
    public Member Member { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalPrice { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public bool CanAcceptPayments => Status is SubscriptionStatus.Pending or SubscriptionStatus.Active;

    private Subscription()
    {
    }

    private Subscription(
        Member member,
        Plan plan,
        DateTime startAt,
        DateTime endAt,
        decimal originalPrice,
        decimal discountAmount,
        SubscriptionStatus status)
    {
        Member = member;
        Plan = plan;
        StartAt = startAt;
        EndAt = endAt;
        OriginalPrice = originalPrice;
        DiscountAmount = discountAmount;
        FinalPrice = CalculateFinalPrice(originalPrice, discountAmount);
        Status = status;
    }

    public static Result<Subscription> Create(
        Member member,
        Plan plan,
        DateTime startAt,
        DateTime endAt,
        decimal originalPrice,
        decimal discountAmount,
        SubscriptionStatus status)
    {
        var error = Validate(member, plan, startAt, endAt, originalPrice, discountAmount, status);
        if (error is not null)
        {
            return error;
        }

        return new Subscription(member, plan, startAt, endAt, originalPrice, discountAmount, status);
    }

    public Result<Updated> UpdateDetails(
        Plan plan,
        DateTime startAt,
        DateTime endAt,
        decimal originalPrice,
        decimal discountAmount,
        SubscriptionStatus status)
    {
        var error = Validate(Member, plan, startAt, endAt, originalPrice, discountAmount, status);
        if (error is not null)
        {
            return error;
        }

        Plan = plan;
        StartAt = startAt;
        EndAt = endAt;
        OriginalPrice = originalPrice;
        DiscountAmount = discountAmount;
        FinalPrice = CalculateFinalPrice(originalPrice, discountAmount);
        Status = status;

        return Result.Updated;
    }

    public Result<Updated> ChangeStatus(SubscriptionStatus status)
    {
        if (status is SubscriptionStatus.Unknown)
        {
            return SubscriptionError.InvalidStatus;
        }

        Status = status;
        return Result.Updated;
    }

    private static decimal CalculateFinalPrice(decimal originalPrice, decimal discountAmount)
        => originalPrice - discountAmount;

    private static Error? Validate(
        Member member,
        Plan plan,
        DateTime startAt,
        DateTime endAt,
        decimal originalPrice,
        decimal discountAmount,
        SubscriptionStatus status)
    {
        if (member is null)
        {
            return SubscriptionError.MemberRequired;
        }

        if (member.IsDeleted)
        {
            return SubscriptionError.MemberDeleted;
        }

        if (plan is null)
        {
            return SubscriptionError.PlanRequired;
        }

        if (!plan.IsActive)
        {
            return SubscriptionError.PlanInactive;
        }

        if (endAt < startAt)
        {
            return SubscriptionError.InvalidDuration;
        }

        if (originalPrice < 0)
        {
            return SubscriptionError.InvalidOriginalPrice;
        }

        if (discountAmount < 0)
        {
            return SubscriptionError.InvalidDiscountAmount;
        }

        if (discountAmount > originalPrice)
        {
            return SubscriptionError.InvalidFinalPrice;
        }

        if (status is SubscriptionStatus.Unknown)
        {
            return SubscriptionError.InvalidStatus;
        }

        return null;
    }
}
