using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.ApprovePayment;

public sealed record ApprovePaymentCommand(
    int PaymentId)
    : IRequest<Result<Updated>>;

