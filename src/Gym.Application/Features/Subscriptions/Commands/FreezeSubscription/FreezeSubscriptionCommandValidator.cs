using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;

public class FreezeSubscriptionCommandValidator : AbstractValidator<FreezeSubscriptionCommand>
{
    public FreezeSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription Id must be greater than 0.");
        RuleFor(x => x.FreezeDays).InclusiveBetween(1, 14).WithMessage("Freeze days must be between 1 and 14 days.");
    }
}