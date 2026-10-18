using Gym.Application.PromoCodes;
using Gym.Domain.Plans;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.Enums;
using Gym.Tests.Common.Plans;
using Gym.Tests.Common.PromoCodes;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class PromoCodeMapperTests
{
    [Fact]
    public void ToResponse_ExtensionMethod_ShouldMapAllFieldsAccurately()
    {
        var plan = PlanFactory.CreatePlan(title: "VIP Gold").Value;
        ReflectionTestHelper.SetProperty(plan, nameof(Plan.Id), 3);

        var promoCode = PromoCodeFactory.CreatePromoCode(
            code: "PROMO2026",
            planId: 3,
            discountType: DiscountType.Percentage,
            discountValue: 25m,
            maxDiscount: 100m,
            minimumPurchaseAmount: 200m,
            audience: PromoCodeAudience.NewMembers,
            usageLimit: 50).Value;

        ReflectionTestHelper.SetProperty(promoCode, nameof(PromoCode.Id), 9);
        ReflectionTestHelper.SetProperty(promoCode, nameof(PromoCode.Plan), plan);
        promoCode.RecordUsage();

        var response = promoCode.ToResponse();

        Assert.Equal(9, response.Id);
        Assert.Equal("PROMO2026", response.Code);
        Assert.Equal(3, response.PlanId);
        Assert.Equal("VIP Gold", response.PlanName);
        Assert.Equal(DiscountType.Percentage, response.DiscountType);
        Assert.Equal(25m, response.DiscountValue);
        Assert.Equal(100m, response.MaxDiscount);
        Assert.Equal(200m, response.MinimumPurchaseAmount);
        Assert.Equal(PromoCodeAudience.NewMembers, response.Audience);
        Assert.Equal(50, response.UsageLimit);
        Assert.Equal(1, response.UsageCount);
        Assert.Equal(promoCode.ExpiresAtUtc, response.ExpiresAtUtc);
        Assert.True(response.IsActive);
    }

    [Fact]
    public void ToResponse_Expression_ShouldCompileAndMapProperties()
    {
        var plan = PlanFactory.CreatePlan(title: "Standard Plan").Value;
        ReflectionTestHelper.SetProperty(plan, nameof(Plan.Id), 1);

        var promoCode = PromoCodeFactory.CreatePromoCode(planId: 1).Value;
        ReflectionTestHelper.SetProperty(promoCode, nameof(PromoCode.Id), 15);
        ReflectionTestHelper.SetProperty(promoCode, nameof(PromoCode.Plan), plan);

        var compiledExpr = PromoCodeMapper.ToResponse().Compile();
        var response = compiledExpr(promoCode);

        Assert.Equal(15, response.Id);
        Assert.Equal(promoCode.Code, response.Code);
        Assert.Equal(1, response.PlanId);
        Assert.Equal("Standard Plan", response.PlanName);
        Assert.Equal(promoCode.DiscountType, response.DiscountType);
        Assert.Equal(promoCode.DiscountValue, response.DiscountValue);
        Assert.Equal(promoCode.IsActive, response.IsActive);
    }
}
