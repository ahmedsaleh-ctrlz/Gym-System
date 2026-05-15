using FluentValidation;

namespace Gym.Application.Features.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.memberId).GreaterThan(0).WithMessage("Member ID must be greater than 0.");
        RuleFor(x => x.plan).NotNull().WithMessage("Plan is required.");
        RuleFor(x => x.startDate).NotEmpty().WithMessage("Start date is required.");
    }
}

