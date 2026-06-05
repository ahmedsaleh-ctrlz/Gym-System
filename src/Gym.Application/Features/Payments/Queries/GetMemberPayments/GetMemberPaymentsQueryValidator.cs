using FluentValidation;

namespace Gym.Application.Features.Payments.Queries.GetMemberPayments
{
    public sealed class GetMemberPaymentsQueryValidator : AbstractValidator<GetMemberPaymentsQuery>
    {
        public GetMemberPaymentsQueryValidator()
        {
            RuleFor(x => x.memberId).GreaterThan(0).WithMessage("Member ID must be greater than 0.");
        }
    }
}
