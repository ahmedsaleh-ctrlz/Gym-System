
using FluentValidation;


namespace Gym.Application.Features.Coaches.Commands.DeleteCoach;

public sealed class DeleteCoachValidator : AbstractValidator<DeleteCoachCommand>
{
    public DeleteCoachValidator()
    {
        RuleFor(x => x.CoachId)
            .GreaterThan(0)
            .WithMessage("MemberId must be greater than 0.");
    }
}
