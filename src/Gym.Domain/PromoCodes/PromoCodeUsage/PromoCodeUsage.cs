using Gym.Domain.Common;
using Gym.Domain.Common.Result;

namespace Gym.Domain.PromoCodes.PromoCodeUsage;

public sealed class PromoCodeUsage : Entity
{
    public int PromoCodeId { get; private set; }
    public PromoCode PromoCode { get; private set; } = null!;

    public int MemberId { get; private set; }

    public DateTime UsedAtUtc { get; private set; }

    private PromoCodeUsage()
    {
    }

    private PromoCodeUsage(
        int promoCodeId,
        int memberId)
    {
        PromoCodeId = promoCodeId;
        MemberId = memberId;
        UsedAtUtc = DateTime.UtcNow;
    }

    public static Result<PromoCodeUsage> Create(
        int promoCodeId,
        int memberId)
    {
        if (promoCodeId <= 0)
        {
            return PromoCodeUsageErrors.InvalidPromoCode;
        }

        if (memberId <= 0)
        {
            return PromoCodeUsageErrors.InvalidMember;
        }

        return new PromoCodeUsage(
            promoCodeId,
            memberId);
    }
}