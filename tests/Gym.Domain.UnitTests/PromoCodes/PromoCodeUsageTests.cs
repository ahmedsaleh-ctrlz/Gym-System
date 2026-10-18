using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.PromoCodeUsage;
using Gym.Tests.Common.PromoCodes;

namespace Gym.Domain.UnitTests.PromoCodes;

public class PromoCodeUsageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldReturnError_WhenPromoCodeIdIsInvalid(int invalidPromoCodeId)
    {
        var result = PromoCodeUsage.Create(invalidPromoCodeId, 1);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeUsageErrors.InvalidPromoCode.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldReturnError_WhenMemberIdIsInvalid(int invalidMemberId)
    {
        var result = PromoCodeUsage.Create(1, invalidMemberId);

        Assert.True(result.IsError);
        Assert.Equal(PromoCodeUsageErrors.InvalidMember.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_AndSetUsedAtUtc_WhenInputsAreValid()
    {
        var before = DateTime.UtcNow;

        var result = PromoCodeUsage.Create(10, 20);

        Assert.True(result.IsSuccess);
        var usage = result.Value;
        Assert.Equal(10, usage.PromoCodeId);
        Assert.Equal(20, usage.MemberId);
        Assert.True(usage.UsedAtUtc >= before.AddSeconds(-1));
    }
}
