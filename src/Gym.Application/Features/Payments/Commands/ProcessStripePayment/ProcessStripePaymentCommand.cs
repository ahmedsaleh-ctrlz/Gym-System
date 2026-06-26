using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.ProcessStripePayment;

public sealed record ProcessStripePaymentCommand(
    string PaymentIntentId)
    : IRequest<Result<Updated>>;