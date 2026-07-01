using FluentValidation;

namespace Gym.Application.Features.Identity.Commands.RegisterMember;

public sealed class RegisterMemberCommandValidator : AbstractValidator<RegisterMemberCommand>
{
    public RegisterMemberCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(20).WithMessage("First name is required and should not exceed 20 characters.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(20).WithMessage("Last name is required and should not exceed 20 characters.");
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Now).WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20).WithMessage("Phone number is required and should not exceed 20 characters.");
        RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("Image URL is required.");
        RuleFor(x => x.Email)
        .EmailAddress().WithMessage("Email format is invalid.")
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().WithMessage("Email format is invalid.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password is required and should be at least 6 characters long.");
    }
}
