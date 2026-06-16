using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Gym.Application.Common.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace Gym.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicaiton(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            });

            return services;
        }
    }
}