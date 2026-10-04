using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestBackgroundJobClient : IBackgroundJobClient
{
    public string Create(Job job, IState state)
    {
        return Guid.NewGuid().ToString();
    }

    public bool Delete(string jobId)
    {
        return true;
    }

    public bool ChangeState(
        string jobId,
        IState state,
        string? expectedState)
    {
        return true;
    }
}