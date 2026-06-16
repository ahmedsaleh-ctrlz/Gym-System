using Gym.Domain.Common.Result;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Subscriptions;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Tests.Common.Payments;

public static class PaymentFactory
{
    public static Result<Payment> CreatePayment(
        Subscription? subscription = null,
        PaymentMethod? paymentMethod = null,
        bool payImmediately = false)
    {
        var effectiveSubscription = subscription;

        if (effectiveSubscription is null)
        {
            var subscriptionResult = SubscriptionFactory.CreateSubscription();
            if (subscriptionResult.IsError)
            {
                return subscriptionResult.Errors;
            }

            effectiveSubscription = subscriptionResult.Value;
        }

        var paymentResult = Payment.Create(effectiveSubscription);
        if (paymentResult.IsError || !payImmediately)
        {
            return paymentResult;
        }

        var payResult = paymentResult.Value.Pay(paymentMethod ?? PaymentMethod.Cash);
        if (payResult.IsError)
        {
            return payResult.Errors;
        }

        return paymentResult.Value;
    }
}