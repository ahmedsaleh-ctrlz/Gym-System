using FluentValidation;

namespace Gym.Application.Features.Attendances.Commands.CheckInMember;

public sealed class CheckInMemberCommandValidator : AbstractValidator<CheckInMemberCommand>
{
    public CheckInMemberCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .WithMessage("MemberId must be greater than 0.");
    }
}