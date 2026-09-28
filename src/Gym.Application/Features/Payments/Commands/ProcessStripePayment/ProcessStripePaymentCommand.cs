using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.ProcessStripePayment;

public sealed record ProcessStripePaymentCommand(
    string PaymentIntentId,
    int? PromoCodeId
) : IRequest<Result<Updated>>;