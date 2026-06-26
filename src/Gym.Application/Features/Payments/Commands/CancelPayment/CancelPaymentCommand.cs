using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.CancelPayment;

public sealed record CancelPaymentCommand(int PaymentId) : IRequest<Result<Updated>>;
