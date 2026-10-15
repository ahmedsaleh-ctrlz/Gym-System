namespace Gym.Application.Common.Interfaces;

public interface IAiService
{
    Task<string> GenerateInsightsAsync(
        string instructions,
        string analyticsContext,
        CancellationToken cancellationToken = default);
}