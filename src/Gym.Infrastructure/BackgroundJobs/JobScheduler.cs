using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.BackgroundJobs;

public static class JobScheduler
{
    public static void RegisterRecurringJobs(IRecurringJobManager recurringJobs)
    {
        recurringJobs.AddOrUpdate<SubscriptionJobs>(
            "activate-scheduled-subscriptions",
            job => job.ActivateScheduledSubscriptions(),
            Cron.Daily(0));

        recurringJobs.AddOrUpdate<SubscriptionJobs>(
            "expire-subscriptions",
            job => job.ExpireSubscriptions(),
            Cron.Daily(0));

        recurringJobs.AddOrUpdate<SubscriptionJobs>(
            "unfreeze-subscriptions",
            job => job.UnfreezeSubscriptions(),
            Cron.Daily(0));
    }
}
