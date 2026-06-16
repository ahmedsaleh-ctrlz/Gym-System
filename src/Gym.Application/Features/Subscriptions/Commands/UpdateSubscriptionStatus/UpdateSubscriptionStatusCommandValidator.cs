using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Commands.UpdateSubscriptionStatus;

public class UpdateSubscriptionStatusCommandValidator : AbstractValidator<UpdateSubscriptionStatusCommand>
{
    public UpdateSubscriptionStatusCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).GreaterThan(0).WithMessage("Subscription ID must be greater than 0.");
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid subscription status.");
    }
}