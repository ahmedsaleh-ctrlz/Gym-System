using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;
using Gym.Tests.Common.PromoCodes;

namespace Gym.Domain.UnitTests.PromoCodes;

public class PromoCodeTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldReturnError_WhenCodeIsInvalid(string? invalidCode)
    {
        var result = PromoCodeFactory.CreatePromoCode(code: invalidCode!);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.CodeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldReturnError_WhenPlanIdIsInvalid(int invalidPlanId)
    {
        var result = PromoCodeFactory.CreatePromoCode(planId: invalidPlanId);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidPlan.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_ShouldReturnError_WhenDiscountValueIsZeroOrNegative(decimal invalidDiscountValue)
    {
        var result = PromoCodeFactory.CreatePromoCode(discountValue: invalidDiscountValue);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidDiscountValue.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPercentageDiscountExceeds100()
    {
        var result = PromoCodeFactory.CreatePromoCode(
            discountType: DiscountType.Percentage,
            discountValue: 101m);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidPercentage.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_ShouldReturnError_WhenMaxDiscountIsZeroOrNegative(decimal invalidMaxDiscount)
    {
        var result = PromoCodeFactory.CreatePromoCode(
            discountType: DiscountType.Percentage,
            discountValue: 20m,
            maxDiscount: invalidMaxDiscount);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidMaxDiscount.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenMaxDiscountProvidedForFixedAmount()
    {
        var result = PromoCodeFactory.CreatePromoCode(
            discountType: DiscountType.FixedAmount,
            discountValue: 50m,
            maxDiscount: 20m);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.MaxDiscountNotAllowedForFixedAmount.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenMinimumPurchaseAmountIsNegative()
    {
        var result = PromoCodeFactory.CreatePromoCode(minimumPurchaseAmount: -1m);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidMinimumPurchaseAmount.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Create_ShouldReturnError_WhenUsageLimitIsInvalid(int invalidUsageLimit)
    {
        var result = PromoCodeFactory.CreatePromoCode(usageLimit: invalidUsageLimit);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidUsageLimit.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenExpirationDateIsInPast()
    {
        var result = PromoCodeFactory.CreatePromoCode(expiresAtUtc: DateTime.UtcNow.AddMinutes(-5));

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeErrors.InvalidExpirationDate.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WithTrimmedAndUpperCode_WhenInputsAreValid()
    {
        var expiry = DateTime.UtcNow.AddDays(15);
        var result = PromoCodeFactory.CreatePromoCode(
            code: "  summer2026  ",
            planId: 2,
            discountType: DiscountType.Percentage,
            discountValue: 25m,
            maxDiscount: 100m,
            minimumPurchaseAmount: 200m,
            audience: PromoCodeAudience.NewMembers,
            usageLimit: 50,
            expiresAtUtc: expiry);

        Assert.True(result.IsSuccess);
        var promoCode = result.Value;
        Assert.Equal("SUMMER2026", promoCode.Code);
        Assert.Equal(2, promoCode.PlanId);
        Assert.Equal(DiscountType.Percentage, promoCode.DiscountType);
        Assert.Equal(25m, promoCode.DiscountValue);
        Assert.Equal(100m, promoCode.MaxDiscount);
        Assert.Equal(200m, promoCode.MinimumPurchaseAmount);
        Assert.Equal(PromoCodeAudience.NewMembers, promoCode.Audience);
        Assert.Equal(50, promoCode.UsageLimit);
        Assert.Equal(0, promoCode.UsageCount);
        Assert.Equal(expiry, promoCode.ExpiresAtUtc);
        Assert.True(promoCode.IsActive);
    }

    [Fact]
    public void ActivateAndDeactivate_ShouldTransitionStateCorrectly()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode().Value;
        Assert.True(promoCode.IsActive);

        var deactResult = promoCode.Deactivate();
        Assert.True(deactResult.IsSuccess);
        Assert.False(promoCode.IsActive);

        var alreadyInactiveResult = promoCode.Deactivate();
        Assert.True(alreadyInactiveResult.IsError);
        Assert.Equal(PromoCodeErrors.AlreadyInactive.Code, alreadyInactiveResult.TopError.Code);

        var actResult = promoCode.Activate();
        Assert.True(actResult.IsSuccess);
        Assert.True(promoCode.IsActive);

        var alreadyActiveResult = promoCode.Activate();
        Assert.True(alreadyActiveResult.IsError);
        Assert.Equal(PromoCodeErrors.AlreadyActive.Code, alreadyActiveResult.TopError.Code);
    }

    [Fact]
    public void CanBeAppliedTo_ShouldReturnNotActive_WhenPromoCodeIsInactive()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1, minimumPurchaseAmount: 100m).Value;
        promoCode.Deactivate();

        var error = promoCode.CanBeAppliedTo(1, 150m);

        Assert.NotNull(error);
        Assert.Equal(PromoCodeErrors.NotActive.Code, error.Value.Code);
    }

    [Fact]
    public void CanBeAppliedTo_ShouldReturnNotApplicableToPlan_WhenPlanIdDoesNotMatch()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1, minimumPurchaseAmount: 100m).Value;

        var error = promoCode.CanBeAppliedTo(2, 150m);

        Assert.NotNull(error);
        Assert.Equal(PromoCodeErrors.NotApplicableToPlan.Code, error.Value.Code);
    }

    [Fact]
    public void CanBeAppliedTo_ShouldReturnMinimumPurchaseAmountNotMet_WhenAmountIsLessThanMinimum()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1, minimumPurchaseAmount: 100m).Value;

        var error = promoCode.CanBeAppliedTo(1, 99.99m);

        Assert.NotNull(error);
        Assert.Equal(PromoCodeErrors.MinimumPurchaseAmountNotMet.Code, error.Value.Code);
    }

    [Fact]
    public void CanBeAppliedTo_ShouldReturnUsageLimitReached_WhenUsageLimitIsReached()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1, usageLimit: 1).Value;
        promoCode.RecordUsage();

        var error = promoCode.CanBeAppliedTo(1, 200m);

        Assert.NotNull(error);
        Assert.Equal(PromoCodeErrors.UsageLimitReached.Code, error.Value.Code);
    }

    [Fact]
    public void CanBeAppliedTo_ShouldReturnNull_WhenAllConditionsAreMet()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1, minimumPurchaseAmount: 100m).Value;

        var error = promoCode.CanBeAppliedTo(1, 150m);

        Assert.Null(error);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-50, 0)]
    public void CalculateDiscount_ShouldReturnZero_WhenAmountIsZeroOrNegative(decimal amount, decimal expected)
    {
        var promoCode = PromoCodeFactory.CreatePromoCode().Value;

        var discount = promoCode.CalculateDiscount(amount);

        Assert.Equal(expected, discount);
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnFixedAmount_CappedByTotalAmount()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(
            discountType: DiscountType.FixedAmount,
            discountValue: 150m,
            maxDiscount: null).Value;

        Assert.Equal(150m, promoCode.CalculateDiscount(500m));
        Assert.Equal(100m, promoCode.CalculateDiscount(100m)); // capped at amount
    }

    [Fact]
    public void CalculateDiscount_ShouldReturnPercentage_AndRespectMaxDiscountCap()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(
            discountType: DiscountType.Percentage,
            discountValue: 50m,
            maxDiscount: 100m).Value;

        // 50% of 100 = 50 (below cap of 100)
        Assert.Equal(50m, promoCode.CalculateDiscount(100m));

        // 50% of 400 = 200 (capped at 100)
        Assert.Equal(100m, promoCode.CalculateDiscount(400m));
    }

    [Fact]
    public void RecordUsage_ShouldIncrementUsageCount_AndRejectWhenLimitExceeded()
    {
        var promoCode = PromoCodeFactory.CreatePromoCode(usageLimit: 2).Value;

        var first = promoCode.RecordUsage();
        Assert.True(first.IsSuccess);
        Assert.Equal(1, promoCode.UsageCount);

        var second = promoCode.RecordUsage();
        Assert.True(second.IsSuccess);
        Assert.Equal(2, promoCode.UsageCount);

        var third = promoCode.RecordUsage();
        Assert.True(third.IsError);
        Assert.Equal(PromoCodeErrors.UsageLimitReached.Code, third.TopError.Code);
        Assert.Equal(2, promoCode.UsageCount); // not incremented past limit
    }
}
