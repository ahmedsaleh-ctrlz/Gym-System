using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetMemberPayments
{
    public sealed record GetMemberPaymentsQuery(int MemberId) : IRequest<Result<List<PaymentResponse>>>;
}