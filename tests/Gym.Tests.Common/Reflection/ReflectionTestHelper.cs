using System.Reflection;

namespace Gym.Tests.Common.Reflection;

public static class ReflectionTestHelper
{
    public static void SetProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
    {
        typeof(TTarget)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}