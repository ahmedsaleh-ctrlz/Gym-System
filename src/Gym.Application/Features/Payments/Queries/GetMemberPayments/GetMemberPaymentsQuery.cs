using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Payments.Queries.GetMemberPayments
{
    public sealed record GetMemberPaymentsQuery(int memberId) : IRequest<Result<List<PaymentResponse>>>;
}
