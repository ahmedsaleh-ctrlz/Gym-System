
using Gym.Domain.People;
using Microsoft.AspNetCore.Identity;

namespace Gym.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public int? PersonId { get; set; }

    public Person Person { get; set; } = null!;
}
