using Gym.Application.Common.Interfaces;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestCurrentUser : IUser
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();

    public string? PersonId { get; set; }

    public string? Role { get; set; } = "Admin";
}
