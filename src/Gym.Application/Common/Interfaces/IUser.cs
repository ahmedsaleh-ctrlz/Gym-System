namespace Gym.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    string? PersonId { get; }
    string? Role { get; }
}