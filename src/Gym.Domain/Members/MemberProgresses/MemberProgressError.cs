using Gym.Domain.Common.Result;

namespace Gym.Domain.MemberProgresses;

public static class MemberProgressError
{
    public static Error MemberRequired => Error.Validation("Member_Progress_Member_Required", "MemberProgressMemberRequired");
    public static Error MemberDeleted => Error.Conflict("Member_Progress_Member_Deleted", "MemberProgressMemberDeleted");
    public static Error InvalidWeight => Error.Validation("Member_Progress_Weight_Invalid", "MemberProgressWeightInvalid");
    public static Error InvalidHeight => Error.Validation("Member_Progress_Height_Invalid", "MemberProgressHeightInvalid");
    public static Error InvalidBodyFat => Error.Validation("Member_Progress_Body_Fat_Invalid", "MemberProgressBodyFatInvalid");
    public static Error InvalidRecordedAt => Error.Validation("Member_Progress_Recorded_At_Invalid", "MemberProgressRecordedAtInvalid");
}
