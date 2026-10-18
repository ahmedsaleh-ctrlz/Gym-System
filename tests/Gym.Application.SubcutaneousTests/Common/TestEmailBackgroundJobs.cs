using Gym.Application.Common.Interfaces.BackgroundJobsServices;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestEmailBackgroundJobs : IEmailBackgroundJobs
{
    public void SendToUsersAsync(
        IEnumerable<string> emails,
        string subject,
        string body)
    {
    }
}
