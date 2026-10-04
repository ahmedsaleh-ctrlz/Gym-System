using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.RejectPayment;

public sealed record RejectPaymentCommand(
    int PaymentId)
    : IRequest<Result<Updated>>;