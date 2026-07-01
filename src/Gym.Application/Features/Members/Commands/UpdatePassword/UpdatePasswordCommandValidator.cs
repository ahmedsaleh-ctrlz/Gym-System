using FluentValidation;

namespace Gym.Application.Features.Members.Commands.UpdatePassword;

public sealed class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0).WithMessage("Member ID must be greater than 0.");
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("New password is required and should be at least 6 characters long.");
    }
}
