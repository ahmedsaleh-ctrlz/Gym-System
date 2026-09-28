using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Common.Interfaces.BackgroundJobsServices
{
    public interface IEmailBackgroundJobs
    {
        void SendToUsersAsync(
        IEnumerable<string> emails,
        string subject,
        string body);
    }
}
