using FluentValidation;

namespace Gym.Application.Features.Members.Commands.DeleteMember;

public sealed class DeleteMemberValidator : AbstractValidator<DeleteMemberCommand>
{
    public DeleteMemberValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .WithMessage("MemberId must be greater than 0.");
    }
}