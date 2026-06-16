using Gym.Application.Features.Payments.Dtos;
using Gym.Domain.Payments;

namespace Gym.Application.Features.Payments.Mappers;

public static class PaymentMapper
{
    public static PaymentResponse ToDto(this Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return new PaymentResponse
        {
            PaymentId = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            MemberId = payment.Subscription.MemberId,
            MemberName = $"{payment.Subscription.Member.Person.FirstName} {payment.Subscription.Member.Person.LastName}",
            PlanName = payment.Subscription.Plan!.Title,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            Status = payment.Status.ToString(),
            PaidAtUtc = payment.PaidAtUtc
        };
    }
}