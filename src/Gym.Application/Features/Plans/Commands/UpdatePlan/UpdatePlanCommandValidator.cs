using FluentValidation;

namespace Gym.Application.Features.Plans.Commands.UpdatePlan;

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .GreaterThan(0)
            .WithMessage("PlanId must be greater than 0.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Plan title is required.")
            .MaximumLength(100).WithMessage("Plan title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Plan cost must be greater than or equal to 0.");

        RuleFor(x => x.DurationInDays)
            .GreaterThan(0).WithMessage("Duration in days must be greater than 0.");
    }
}
