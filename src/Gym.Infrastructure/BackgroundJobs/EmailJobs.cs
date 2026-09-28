using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Domain.Notifications.Enums;
using Gym.Infrastructure.Notifications;

using Hangfire;

namespace Gym.Infrastructure.BackgroundJobs
{
    public sealed class EmailJobs(IEmailSender emailSender)
    {
        public async Task SendToUsersAsync(
            IEnumerable<string> emails,
            string subject,
            string htmlBody)
        {
            try
            {
                await emailSender.SendEmailsToUsersAsync(
                emails,
                subject,
                htmlBody);
            }
            catch
            {
                throw new InvalidDataException();
            }
        }
    }
}
