using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes.Enums;

namespace Gym.Domain.PromoCodes;

public sealed class PromoCode : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public decimal DiscountAmount { get; private set; }
    public int UsedQuantity { get; private set; }
    public int MaxQuantity { get; private set; }
    public DateTime StartDuration { get; private set; }
    public DateTime EndDuration { get; private set; }
    public PromoCodeType Type { get; private set; }

    private PromoCode()
    {
    }

    private PromoCode(
        string code,
        decimal discountAmount,
        int usedQuantity,
        int maxQuantity,
        DateTime startDuration,
        DateTime endDuration,
        PromoCodeType type)
    {
        Code = code;
        DiscountAmount = discountAmount;
        UsedQuantity = usedQuantity;
        MaxQuantity = maxQuantity;
        StartDuration = startDuration;
        EndDuration = endDuration;
        Type = type;
    }

    public static Result<PromoCode> Create(
        string code,
        decimal discountAmount,
        int usedQuantity,
        int maxQuantity,
        DateTime startDuration,
        DateTime endDuration,
        PromoCodeType type)
    {
        var error = Validate(code, discountAmount, usedQuantity, maxQuantity, startDuration, endDuration, type);
        if (error is not null)
        {
            return error;
        }

        return new PromoCode(code, discountAmount, usedQuantity, maxQuantity, startDuration, endDuration, type);
    }

    public Result<Updated> UpdateInfo(
        string code,
        decimal discountAmount,
        int usedQuantity,
        int maxQuantity,
        DateTime startDuration,
        DateTime endDuration,
        PromoCodeType type)
    {
        var error = Validate(code, discountAmount, usedQuantity, maxQuantity, startDuration, endDuration, type);
        if (error is not null)
        {
            return error;
        }

        Code = code;
        DiscountAmount = discountAmount;
        UsedQuantity = usedQuantity;
        MaxQuantity = maxQuantity;
        StartDuration = startDuration;
        EndDuration = endDuration;
        Type = type;

        return Result.Updated;
    }

    public Result<Updated> RegisterUsage(DateTime usedAt)
    {
        var error = ValidateUsage(usedAt);
        if (error is not null)
        {
            return error;
        }

        UsedQuantity++;
        return Result.Updated;
    }

    private static Error? Validate(
        string code,
        decimal discountAmount,
        int usedQuantity,
        int maxQuantity,
        DateTime startDuration,
        DateTime endDuration,
        PromoCodeType type)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return PromoCodeError.CodeRequired;
        }

        if (discountAmount < 0)
        {
            return PromoCodeError.InvalidDiscountAmount;
        }

        if (maxQuantity <= 0)
        {
            return PromoCodeError.InvalidMaxQuantity;
        }

        if (usedQuantity < 0 || usedQuantity > maxQuantity)
        {
            return PromoCodeError.InvalidUsedQuantity;
        }

        if (endDuration < startDuration)
        {
            return PromoCodeError.InvalidDuration;
        }

        if (type is PromoCodeType.Unknown)
        {
            return PromoCodeError.InvalidType;
        }

        return null;
    }

    private Error? ValidateUsage(DateTime usedAt)
    {
        if (usedAt < StartDuration || usedAt > EndDuration)
        {
            return PromoCodeError.PromoCodeInactive;
        }

        if (UsedQuantity >= MaxQuantity)
        {
            return PromoCodeError.PromoCodeUsageExceeded;
        }

        return null;
    }
}
