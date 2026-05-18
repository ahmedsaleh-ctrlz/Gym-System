using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions.Enums;


namespace Gym.Domain.Subscriptions;
public class Subscription : AuditableEntity
{

    public int MemberId { get; private set; }
    public Member Member { get; private set; }
    public int PlanId { get; private set; }
    public Plan? Plan { get; private set; } 
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public int FreezeCountUsed { get; private set; }
    public int TotalFreezeDaysUsed { get; private set; }
    private DateOnly? FreezeEndDate { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Subscription() { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Subscription(int memberId, Plan plan, DateOnly startDate)

    {
        MemberId = memberId;
        Plan = plan;
        PlanId = plan.Id;
        StartDate = startDate;
        EndDate = StartDate.AddDays(Plan!.DurationInDays);
        Status = SubscriptionStatus.Pending;
        FreezeCountUsed = 0;
        TotalFreezeDaysUsed = 0;
    }   

    public static Result<Subscription> Create(int memberId, Plan plan, DateOnly startDate)
    {
        var error = Validate(memberId, plan.Id, startDate);
        if (error is not null)
            return error;

        return new Subscription(memberId, plan, startDate);
    }

    public Result<Updated> Activate()
    {
        if (Status != SubscriptionStatus.Pending)
            return SubscriptionErrors.OnlyPendingSubscriptionsCanBeActivated;
        Status = SubscriptionStatus.Active;
        return Result.Updated;
    }
    public Result<Updated> Scheduled()
    {
        if (Status != SubscriptionStatus.Pending)
            return SubscriptionErrors.OnlyPendingSubscriptionsCanBeScheduled;
        Status = SubscriptionStatus.Scheduled;
        return Result.Updated;
    }

    public Result<Updated> Freeze(int FreezeDays)
    {
        var error = ValidFreezeErrorResult(FreezeDays);
        if (error is not null)
            return error;

        Status = SubscriptionStatus.Frozen;
        FreezeCountUsed += 1;
        TotalFreezeDaysUsed += FreezeDays;
        EndDate = EndDate.AddDays(FreezeDays);
        FreezeEndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(FreezeDays); 
        return Result.Updated;
    }

    public Result<Updated> Cancel()
    {
        if (Status != SubscriptionStatus.Active)
            return SubscriptionErrors.OnlyActiveSubscriptionsCanBeCancelled;
        Status = SubscriptionStatus.Cancelled;
        return Result.Updated;
    }
    public Result<Updated> Unfreeze()
    {
        if (Status != SubscriptionStatus.Frozen)
            return SubscriptionErrors.OnlyFrozenSubscriptionsCanBeUnfrozen;
        if (FreezeEndDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return SubscriptionErrors.FreezeEndDateNotNow;
        FreezeEndDate = null;
        Status = SubscriptionStatus.Active;
        return Result.Updated;
    }

    public bool CanUnFreeze()
    {
        if (Status != SubscriptionStatus.Frozen)
            return false;
        if (FreezeEndDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return false;
        return true;
    }

    private static Error? Validate(int memberId, int planId, DateOnly startDate)
    {
        if (memberId <= 0)
            return SubscriptionErrors.SubscriptionShouldAssignToMember;
        if (planId <= 0)
            return SubscriptionErrors.SubscriptionShouldAssignToPlan;
        if(startDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return SubscriptionErrors.SubscriptionStartDateCannotBeInThePast;
        
        return null;
    }
    private Error? ValidFreezeErrorResult(int FreezeDays)
    {
        if (FreezeCountUsed >= Plan!.AllowedFreezeCount)
            return SubscriptionErrors.CannotFreezeMoreThanAllowedFreezeCount;

        if (TotalFreezeDaysUsed + FreezeDays > Plan.MaxTotalFreezeDays)
            return SubscriptionErrors.CannotFreezeMoreThanAllowedFreezeDays;

        if (Status != SubscriptionStatus.Active)
            return SubscriptionErrors.OnlyActiveSubscriptionsCanBeFrozen;

        return null;
    }

}

