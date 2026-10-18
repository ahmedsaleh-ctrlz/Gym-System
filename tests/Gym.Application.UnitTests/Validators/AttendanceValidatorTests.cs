using FluentValidation.TestHelper;
using Gym.Application.Features.Attendances.Commands.CheckInMember;

namespace Gym.Application.UnitTests.Validators;

public class AttendanceValidatorTests
{
    private readonly CheckInMemberCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CheckInMemberCommandValidator_ShouldHaveValidationError_WhenMemberIdIsZeroOrNegative(int invalidMemberId)
    {
        var command = new CheckInMemberCommand(invalidMemberId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MemberId)
              .WithErrorMessage("MemberId must be greater than 0.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void CheckInMemberCommandValidator_ShouldNotHaveValidationError_WhenMemberIdIsPositive(int validMemberId)
    {
        var command = new CheckInMemberCommand(validMemberId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MemberId);
    }
}
