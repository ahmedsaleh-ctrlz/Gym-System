using FluentValidation.TestHelper;
using Gym.Application.Features.Subscriptions.Commands.CreateSubscription;
using Gym.Application.Features.Subscriptions.Commands.FreezeSubscription;

namespace Gym.Application.UnitTests.Validators;

public class SubscriptionValidatorTests
{
    private readonly CreateSubscriptionCommandValidator _createValidator = new();
    private readonly FreezeSubscriptionCommandValidator _freezeValidator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateSubscriptionValidator_ShouldFail_WhenMemberIdIsZeroOrNegative(int invalidMemberId)
    {
        var command = new CreateSubscriptionCommand(invalidMemberId, 1, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MemberId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateSubscriptionValidator_ShouldFail_WhenPlanIdIsZeroOrNegative(int invalidPlanId)
    {
        var command = new CreateSubscriptionCommand(1, invalidPlanId, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PlanId);
    }

    [Fact]
    public void CreateSubscriptionValidator_ShouldPass_WhenInputsAreValid()
    {
        var command = new CreateSubscriptionCommand(1, 2, DateOnly.FromDateTime(DateTime.UtcNow));

        var result = _createValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(15)]
    [InlineData(30)]
    public void FreezeSubscriptionValidator_ShouldFail_WhenFreezeDaysAreOutOfRange(int invalidDays)
    {
        var command = new FreezeSubscriptionCommand(1, invalidDays);

        var result = _freezeValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FreezeDays);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    public void FreezeSubscriptionValidator_ShouldPass_WhenFreezeDaysAreWithinRange(int validDays)
    {
        var command = new FreezeSubscriptionCommand(1, validDays);

        var result = _freezeValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
