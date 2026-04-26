
namespace Gym.Domain.People.PersonImages;
public sealed class PersonImage
{
    public int Id { get; }
    public int PersonId { get; }
    public string ImageUrl { get; private set; } = string.Empty;
}
