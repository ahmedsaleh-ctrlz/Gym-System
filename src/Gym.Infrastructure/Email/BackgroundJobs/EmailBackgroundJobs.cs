using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces.BackgroundJobsServices;
using Gym.Infrastructure.BackgroundJobs;

using Hangfire;

namespace Gym.Infrastructure.Email.NewFolder
{
    public class EmailBackgroundJobs(IBackgroundJobClient backgroundJobClient) : IEmailBackgroundJobs
    {
        public void SendToUsersAsync(IEnumerable<string> emails, string subject, string body)
        {
            backgroundJobClient.Enqueue<EmailJobs>(
                job => job.SendToUsersAsync(emails, subject, body));
        }
    }
}
