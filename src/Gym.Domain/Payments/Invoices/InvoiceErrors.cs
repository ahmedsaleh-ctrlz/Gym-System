using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Common.Result;

namespace Gym.Domain.Payments.Invoices
{
    public static class InvoiceErrors
    {
        public static Error InvoiceNumberIsRequired =>
            Error.Validation(
                "Invoices.Validation",
                "Invoice number is required.");

        public static Error PaymentIsInvalid =>
        Error.Validation(
            "Invoices.Validation",
            "Payment is invalid.");

        public static Error PlanIsInvalid =>
            Error.Validation(
                "Invoices.Validation",
                "Subscription plan is invalid.");

        public static Error MemberIsInvalid =>
            Error.Validation(
                "Invoices.Validation",
                "Member information is invalid.");
    }
}
