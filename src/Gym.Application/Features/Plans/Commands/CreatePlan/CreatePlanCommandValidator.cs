using FluentValidation;

namespace Gym.Application.Features.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(p => p.Title).NotEmpty().WithMessage("Plan Title Is Required");
        RuleFor(p => p.Cost).NotEmpty().WithMessage("Plan Price Is Required");
        RuleFor(p => p.DurationInDays).NotEmpty().WithMessage("Duration Required");
        RuleFor(p => p.AllowedFreezeCount).NotEmpty().WithMessage("Allowed Freeze Count Required");
        RuleFor(p => p.MaxTotalFreezeDays).NotEmpty().WithMessage("Max Total Freeze Days Required");
    }
}