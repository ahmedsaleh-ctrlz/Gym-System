using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;

public class UpdateSubscriptionStatusCommandValidator : AbstractValidator<UpdateSubscriptionStatusCommand>
{
    public UpdateSubscriptionStatusCommandValidator()
    {
        RuleFor(x => x.subscriptionId).GreaterThan(0).WithMessage("Subscription ID must be greater than 0.");
        RuleFor(x => x.newStatus).IsInEnum().WithMessage("Invalid subscription status.");
    }
}