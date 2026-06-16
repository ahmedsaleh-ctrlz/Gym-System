using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Queries.GetMemberSubscriptions
{
    public sealed class GetMemberSubscriptionsQueryValidator : AbstractValidator<GetMemberSubscriptionsQuery>
    {
        public GetMemberSubscriptionsQueryValidator()
        {
            RuleFor(x => x.MemberId).GreaterThan(0).WithMessage("Member ID must be greater than 0.");
        }
    }
}