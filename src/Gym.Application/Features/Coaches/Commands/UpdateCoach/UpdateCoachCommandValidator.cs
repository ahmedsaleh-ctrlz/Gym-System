using FluentValidation;

namespace Gym.Application.Features.Coaches.Commands.UpdateCoach;
public class UpdateCoachCommandValidator : AbstractValidator<UpdateCoachCommand>
{
    public UpdateCoachCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in a valid format.");
      
    }


}
public sealed class UpdateCoachImageCommandValidator : AbstractValidator<UpdateCoachImageCommand>
{
    public UpdateCoachImageCommandValidator()
    {
        RuleFor(x => x.imageUrl)
            .NotEmpty().WithMessage("Image URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Image URL must be a valid URL.");
    }
}
