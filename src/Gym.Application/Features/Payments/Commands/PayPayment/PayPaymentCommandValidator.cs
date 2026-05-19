using FluentValidation;


namespace Gym.Application.Features.Payments.Commands.PayPayment;

public sealed class PayPaymentCommandValidator : AbstractValidator<PayPaymentCommand>
{
    public PayPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0).WithMessage("PaymentId must be greater than 0.");
        RuleFor(x => x.paymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");
    }
}