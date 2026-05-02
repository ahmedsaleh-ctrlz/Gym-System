namespace Gym.Client.Models;

public sealed class PaginatedList<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyCollection<T>? Items { get; set; }
}
