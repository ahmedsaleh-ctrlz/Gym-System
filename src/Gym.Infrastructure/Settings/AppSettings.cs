using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Settings
{
    public class AppSettings
    {
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public string CorsPolicyName { get; set; } = default!;
        public string[] AllowedOrigins { get; set; } = default!;
    }
}
