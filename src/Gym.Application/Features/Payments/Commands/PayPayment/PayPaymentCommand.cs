using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

using MediatR;

namespace Gym.Application.Features.Payments.Commands.PayPayment;

public sealed record PayPaymentCommand(int PaymentId, PaymentMethod PaymentMethod) : IRequest<Result<Updated>>;