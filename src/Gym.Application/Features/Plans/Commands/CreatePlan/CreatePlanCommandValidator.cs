using FluentValidation;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(p => p.title).NotEmpty().WithMessage("Plan Title Is Required");
        RuleFor(p => p.cost).NotEmpty().WithMessage("Plan Price Is Required");
        RuleFor(p => p.durationInDays).NotEmpty().WithMessage("Duration Required");

    }
}