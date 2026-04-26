using Gym.Domain.Common;
using Gym.Domain.People;

namespace Gym.Domain.Members;
public sealed class Member  : AuditableEntity
{
    public int Id { get; } 
    public int PersonId { get; }
    public Person person { get; } = null!;

    public DateTime JoinDate { get; private set; }

    public string? Notes { get; private set; }

    public bool IsDeleted { get; private set ;} = false;

    public DateTime DeletedAt { get; private set; }


}
