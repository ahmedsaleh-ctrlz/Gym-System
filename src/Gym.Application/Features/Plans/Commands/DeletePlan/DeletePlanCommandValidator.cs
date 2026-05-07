using FluentValidation;

namespace Gym.Application.Features.Plans.Commands.DeletePlan;

public sealed class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
    public DeletePlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0)
            .WithMessage("PlanId must be greater than 0.");
    }
}
