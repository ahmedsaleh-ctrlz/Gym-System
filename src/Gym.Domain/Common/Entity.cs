namespace Gym.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
    }

    public int Id { get; protected set; }

    private readonly List<DomainEvents> _domainEvents = [];

    public void AddDomainEvent(DomainEvents domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvents domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}