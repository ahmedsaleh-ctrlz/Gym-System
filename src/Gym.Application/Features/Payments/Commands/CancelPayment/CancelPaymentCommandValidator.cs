using FluentValidation;

namespace Gym.Application.Features.Payments.Commands.CancelPayment;

public sealed class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("PaymentId must be greater than 0");
    }
}