using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Infrastructure.Identity.Policies
{
    public static class Policies
    {
        public const string SameCoach = "SameCoach";
        public const string SameMemberOrAdmin = "SameMemberOrAdmin";
        public const string SameMemberOrCoachOrAdmin = "SameMemberOrCoachOrAdmin";

    }
}
