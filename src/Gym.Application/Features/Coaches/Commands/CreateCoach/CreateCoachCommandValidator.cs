using FluentValidation;



namespace Gym.Application.Features.Coaches.Commands.CreateCoach;

public sealed class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
{
    public CreateCoachCommandValidator()
    {
        RuleFor(x => x.firstName).NotEmpty().MaximumLength(20).WithMessage("First name is required and should not exceed 20 characters.");
        RuleFor(x => x.lastName).NotEmpty().MaximumLength(20).WithMessage("Last name is required and should not exceed 20 characters.");
        RuleFor(x => x.dateOfBirth).LessThan(DateTime.Now).WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.phoneNumber).NotEmpty().MaximumLength(20).WithMessage("Phone number is required and should not exceed 20 characters.");
        RuleFor(x => x.imageUrl).NotEmpty().WithMessage("Image URL is required.");
        RuleFor(x => x.email)
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().WithMessage("Email format is invalid.");
        RuleFor(x => x.password).NotEmpty().MinimumLength(6).WithMessage("Password is required and should be at least 6 characters long.");
       
    }
}
