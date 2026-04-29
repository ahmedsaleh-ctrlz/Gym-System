using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Employees;

namespace Gym.Domain.Classes;

public sealed class GymClass : AuditableEntity
{
    public Employee Coach { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime EndAt { get; private set; }

    private GymClass()
    {
    }

    private GymClass(Employee coach, string title, string? description, DateTime startedAt, DateTime endAt)
    {
        Coach = coach;
        Title = title;
        Description = description;
        StartedAt = startedAt;
        EndAt = endAt;
    }

    public static Result<GymClass> Create(Employee coach, string title, string? description, DateTime startedAt, DateTime endAt)
    {
        var error = Validate(coach, title, startedAt, endAt);
        if (error is not null)
        {
            return error;
        }

        return new GymClass(coach, title, description, startedAt, endAt);
    }

    public Result<Updated> UpdateInfo(Employee coach, string title, string? description, DateTime startedAt, DateTime endAt)
    {
        var error = Validate(coach, title, startedAt, endAt);
        if (error is not null)
        {
            return error;
        }

        Coach = coach;
        Title = title;
        Description = description;
        StartedAt = startedAt;
        EndAt = endAt;

        return Result.Updated;
    }

    private static Error? Validate(Employee coach, string title, DateTime startedAt, DateTime endAt)
    {
        if (coach is null)
        {
            return GymClassError.CoachRequired;
        }

        if (!coach.IsActive)
        {
            return GymClassError.CoachInactive;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return GymClassError.TitleRequired;
        }

        if (endAt <= startedAt)
        {
            return GymClassError.InvalidDuration;
        }

        return null;
    }
}
