using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Commands.RenewSubscription;

public sealed class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0).WithMessage("MemberId must be greater than 0.");
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("PlanId must be greater than 0.");
    }
}