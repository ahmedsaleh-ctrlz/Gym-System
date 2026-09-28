using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Payments.Invoices;

namespace Gym.Application.Common.Interfaces
{
    public interface IInvoicePdfGenerator
    {
        byte[] Generate(Invoice invoice);
    }
}
