using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Plans;
using Gym.Domain.PromoCodes.Enums;

namespace Gym.Domain.PromoCodes;

public sealed class PromoCode : Entity
{
    public string Code { get; private set; } = null!;

    public int PlanId { get; private set; }
    public Plan Plan { get; private set; } = null!;

    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MaxDiscount { get; private set; }
    public decimal? MinimumPurchaseAmount { get; private set; }

    public PromoCodeAudience Audience { get; private set; }

    public int UsageLimit { get; private set; }
    public int UsageCount { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    private PromoCode()
    {
    }

    private PromoCode(
        string code,
        int planId,
        DiscountType discountType,
        decimal discountValue,
        decimal? maxDiscount,
        decimal? minimumPurchaseAmount,
        PromoCodeAudience audience,
        int usageLimit,
        DateTime expiresAtUtc)
    {
        Code = code.Trim().ToUpperInvariant();
        PlanId = planId;
        DiscountType = discountType;
        DiscountValue = discountValue;
        MaxDiscount = maxDiscount;
        MinimumPurchaseAmount = minimumPurchaseAmount;
        Audience = audience;
        UsageLimit = usageLimit;
        UsageCount = 0;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public static Result<PromoCode> Create(
        string code,
        int planId,
        DiscountType discountType,
        decimal discountValue,
        decimal? maxDiscount,
        decimal? minimumPurchaseAmount,
        PromoCodeAudience audience,
        int usageLimit,
        DateTime expiresAtUtc)
    {
        var error = Validate(
            code,
            planId,
            discountType,
            discountValue,
            maxDiscount,
            minimumPurchaseAmount,
            usageLimit,
            expiresAtUtc);

        if (error is not null)
        {
            return error;
        }

        return new PromoCode(
            code,
            planId,
            discountType,
            discountValue,
            maxDiscount,
            minimumPurchaseAmount,
            audience,
            usageLimit,
            expiresAtUtc);
    }

    public Result<Updated> Activate()
    {
        if (IsActive)
        {
            return PromoCodeErrors.AlreadyActive;
        }

        IsActive = true;

        return Result.Updated;
    }

    public Result<Updated> Deactivate()
    {
        if (!IsActive)
        {
            return PromoCodeErrors.AlreadyInactive;
        }

        IsActive = false;

        return Result.Updated;
    }

    public Error? CanBeAppliedTo(
        int planId,
        decimal purchaseAmount)
    {
        if (!IsActive)
        {
            return PromoCodeErrors.NotActive;
        }

        if (DateTime.UtcNow >= ExpiresAtUtc)
        {
            return PromoCodeErrors.Expired;
        }

        if (UsageCount >= UsageLimit)
        {
            return PromoCodeErrors.UsageLimitReached;
        }

        if (PlanId != planId)
        {
            return PromoCodeErrors.NotApplicableToPlan;
        }

        if (purchaseAmount < MinimumPurchaseAmount)
        {
            return PromoCodeErrors.MinimumPurchaseAmountNotMet;
        }

        return null;
    }

    public decimal CalculateDiscount(decimal amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var discount = DiscountType switch
        {
            DiscountType.FixedAmount =>
                DiscountValue,

            DiscountType.Percentage =>
                amount * (DiscountValue / 100m),

            _ => 0
        };

        if (MaxDiscount.HasValue)
        {
            discount = Math.Min(
                discount,
                MaxDiscount.Value);
        }

        return Math.Min(discount, amount);
    }

    public Result<Updated> RecordUsage()
    {
        if (UsageCount >= UsageLimit)
        {
            return PromoCodeErrors.UsageLimitReached;
        }

        UsageCount++;

        return Result.Updated;
    }

    private static Error? Validate(
        string code,
        int planId,
        DiscountType discountType,
        decimal discountValue,
        decimal? maxDiscount,
        decimal? minimumPurchaseAmount,
        int usageLimit,
        DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return PromoCodeErrors.CodeRequired;
        }

        if (planId <= 0)
        {
            return PromoCodeErrors.InvalidPlan;
        }

        if (discountValue <= 0)
        {
            return PromoCodeErrors.InvalidDiscountValue;
        }

        if (discountType == DiscountType.Percentage &&
            discountValue > 100)
        {
            return PromoCodeErrors.InvalidPercentage;
        }

        if (maxDiscount.HasValue &&
            maxDiscount.Value <= 0)
        {
            return PromoCodeErrors.InvalidMaxDiscount;
        }

        if (discountType == DiscountType.FixedAmount &&
            maxDiscount.HasValue)
        {
            return PromoCodeErrors.MaxDiscountNotAllowedForFixedAmount;
        }

        if (minimumPurchaseAmount < 0)
        {
            return PromoCodeErrors.InvalidMinimumPurchaseAmount;
        }

        if (usageLimit <= 0)
        {
            return PromoCodeErrors.InvalidUsageLimit;
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return PromoCodeErrors.InvalidExpirationDate;
        }

        return null;
    }
}