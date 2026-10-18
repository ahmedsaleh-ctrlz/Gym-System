using FluentValidation.TestHelper;
using Gym.Application.Features.Plans.Commands.CreatePlan;
using Gym.Application.Features.Plans.Commands.DeletePlan;
using Gym.Application.Features.Plans.Commands.UpdatePlan;

namespace Gym.Application.UnitTests.Validators;

public class PlanValidatorTests
{
    private readonly CreatePlanCommandValidator _createValidator = new();
    private readonly UpdatePlanCommandValidator _updateValidator = new();
    private readonly DeletePlanCommandValidator _deleteValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreatePlanValidator_ShouldFail_WhenTitleIsEmpty(string? invalidTitle)
    {
        var command = new CreatePlanCommand(invalidTitle!, "desc", 100m, 30, 2, 14);

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePlanValidator_ShouldFail_WhenCostIsZero()
    {
        var command = new CreatePlanCommand("Title", "desc", 0m, 30, 2, 14);

        var result = _createValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Cost);
    }

    [Fact]
    public void CreatePlanValidator_ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new CreatePlanCommand("Gold Plan", "Description", 500m, 30, 2, 14);

        var result = _createValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeletePlanValidator_ShouldFail_WhenPlanIdIsZeroOrNegative(int invalidId)
    {
        var command = new DeletePlanCommand(invalidId);

        var result = _deleteValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PlanId);
    }

    [Fact]
    public void DeletePlanValidator_ShouldPass_WhenPlanIdIsPositive()
    {
        var command = new DeletePlanCommand(5);

        var result = _deleteValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PlanId);
    }

    [Fact]
    public void UpdatePlanValidator_ShouldFail_WhenTitleIsEmpty()
    {
        var command = new UpdatePlanCommand(1, string.Empty, "desc", 100m, 30, 2, 14);

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
}
