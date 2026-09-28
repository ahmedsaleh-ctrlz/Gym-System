namespace Gym.Api.Contracts.PromoCodes
{
    public sealed record PreviewPromoCodeRequest(string PromoCode, int MemberId, int PlanId);
}
