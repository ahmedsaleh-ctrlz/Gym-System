using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.MemberProgresses;
using Gym.Domain.Members;

namespace Gym.Domain.Members.MemberProgresses;

public sealed class MemberProgress : AuditableEntity
{
    public Member Member { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public decimal Height { get; private set; }
    public decimal BodyFat { get; private set; }
    public string? Notes { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private MemberProgress()
    {
    }

    private MemberProgress(Member member, decimal weight, decimal height, decimal bodyFat, string? notes, DateTime recordedAt)
    {
        Member = member;
        Weight = weight;
        Height = height;
        BodyFat = bodyFat;
        Notes = notes;
        RecordedAt = recordedAt;
    }

    public static Result<MemberProgress> Create(
        Member member,
        decimal weight,
        decimal height,
        decimal bodyFat,
        string? notes,
        DateTime recordedAt)
    {
        var error = Validate(member, weight, height, bodyFat, recordedAt);
        if (error is not null)
        {
            return error;
        }

        return new MemberProgress(member, weight, height, bodyFat, notes, recordedAt);
    }

    public Result<Updated> UpdateInfo(decimal weight, decimal height, decimal bodyFat, string? notes, DateTime recordedAt)
    {
        var error = Validate(Member, weight, height, bodyFat, recordedAt);
        if (error is not null)
        {
            return error;
        }

        Weight = weight;
        Height = height;
        BodyFat = bodyFat;
        Notes = notes;
        RecordedAt = recordedAt;

        return Result.Updated;
    }

    private static Error? Validate(Member member, decimal weight, decimal height, decimal bodyFat, DateTime recordedAt)
    {
        if (member is null)
        {
            return MemberProgressError.MemberRequired;
        }

        if (weight < 0)
        {
            return MemberProgressError.InvalidWeight;
        }

        if (height < 0)
        {
            return MemberProgressError.InvalidHeight;
        }

        if (bodyFat < 0)
        {
            return MemberProgressError.InvalidBodyFat;
        }

        if (recordedAt > DateTime.UtcNow)
        {
            return MemberProgressError.InvalidRecordedAt;
        }

        return null;
    }
}
