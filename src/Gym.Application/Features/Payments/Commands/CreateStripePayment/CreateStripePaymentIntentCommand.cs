using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.CreateStripePayment;

public sealed record CreateStripePaymentIntentCommand(
    int PaymentId)
    : IRequest<Result<StripePaymentIntentResult>>;
