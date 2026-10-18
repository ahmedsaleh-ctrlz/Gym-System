using FluentValidation.TestHelper;
using Gym.Application.Features.Payments.Commands.CancelPayment;
using Gym.Application.Features.Payments.Commands.PayPayment;
using Gym.Domain.Payments.Enums;

namespace Gym.Application.UnitTests.Validators;

public class PaymentValidatorTests
{
    private readonly PayPaymentCommandValidator _payValidator = new();
    private readonly CancelPaymentCommandValidator _cancelValidator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PayPaymentValidator_ShouldFail_WhenPaymentIdIsZeroOrNegative(int invalidId)
    {
        var command = new PayPaymentCommand(invalidId, PaymentMethod.Cash);

        var result = _payValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentId)
              .WithErrorMessage("PaymentId must be greater than 0.");
    }

    [Fact]
    public void PayPaymentValidator_ShouldFail_WhenPaymentMethodIsInvalidEnum()
    {
        var command = new PayPaymentCommand(1, (PaymentMethod)999);

        var result = _payValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod)
              .WithErrorMessage("Invalid payment method.");
    }

    [Fact]
    public void PayPaymentValidator_ShouldPass_WhenInputsAreValid()
    {
        var command = new PayPaymentCommand(1, PaymentMethod.Visa, "REF123", null);

        var result = _payValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CancelPaymentValidator_ShouldFail_WhenPaymentIdIsInvalid(int invalidId)
    {
        var command = new CancelPaymentCommand(invalidId);

        var result = _cancelValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }
}
