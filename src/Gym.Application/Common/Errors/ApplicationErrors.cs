using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions;

namespace Gym.Application.Common.Errors;

public class ApplicationErrors
{
    public static Error MemberNotFound => Error.NotFound("MemberNotFound", "Member with the specified ID was not found.");
    public static Error CannotDeleteSubscribedMember => Error.Conflict("CannotDeleteSubscribedMember", "Cannot delete a member who is currently subscribed to a plan.");
    public static Error CoachNotFound => Error.NotFound("CoachNotFound", "Coach with the specified ID was not found.");
    public static Error PlanNotFound => Error.NotFound("PlanNotFound", "Plan with the specified ID was not found.");

    public static Error SubscriptionNotFound => Error.NotFound("SubscriptionNotFound", "Subscription with the specified ID was not found.");
    public static Error PaymentNotFound => Error.NotFound("PaymentNotFound", "Payment with the specified ID was not found.");

    public static Error PlanNotActive => Error.Conflict("PlanNotActive", "The selected plan is not active and cannot be subscribed to.");

    public static Error InvalidSubscriptionStartDate => Error.Conflict("Invalid_StartDate", "Start date must be at least 14 days from today");

    public static Error CannotCreateSubscriptionForMemberWithActiveOrFrozenSubscription => Error.Conflict("CannotCreateSubscriptionForMemberWithActiveOrFrozenSubscription", "Cannot create a subscription for a member who already has an active or frozen subscription.");

    public static Error InvalidCheckInTime =>
        Error.Conflict("Attendance.InvalidCheckInTime", "Attendance timestamp cannot be in the future.");

    public static Error MemberCannotCheckInWithoutActiveSubscription =>
        Error.Conflict("Attendance.SubscriptionInactive", "Member cannot check-in without an active subscription.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
        code: "Auth.ExpiredAccessToken.Invalid",
        description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");

    public static readonly Error RefreshTokenRevoked = Error.Conflict(
        code: "Auth.RefreshToken.Revoked",
        description: "Refresh token is invalid or has Revoked.");

    public static Error InvalidPaymentOwner =>
        Error.Forbidden(
            "Payment.Forbidden",
            "You are not allowed to access this payment.");

    public static Error PromoCodeNotFound =>
        Error.NotFound(
            "PromoCode.NotFound",
            "Promo code not found.");

    public static Error PromoCodeNotValid =>
        Error.Conflict(
            "PromoCode.NotValid",
            "Promo code is not valid.");

    public static Error PromoCodeNotApplicableToPlan =>
        Error.Conflict(
            code: "PromoCode.NotApplicableToPlan", description: "Promo code is not applicable to this plan.");

    public static Error MinimumPurchaseAmountNotMet =>
        Error.Conflict(
            "PromoCode.MinimumPurchaseAmountNotMet",
            "The minimum purchase amount required for this promo code has not been met.");

    public static Error PromoCodeAlreadyUsed =>
        Error.Conflict(
            "PromoCode.AlreadyUsed",
            "You have already used this promo code.");

    public static Error PromoCodeOnlyForNewMembers =>
        Error.Conflict(
            "PromoCode.OnlyForNewMembers",
            "This promo code is only available to new members.");

    public static Error InvoiceNotFound => Error.NotFound("Invoice.NotFound", "Invoice with the specified ID was not found.");

}