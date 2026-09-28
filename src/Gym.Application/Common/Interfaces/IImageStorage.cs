namespace Gym.Application.Common.Interfaces;

public interface IImageStorage
{
    Task<string> SaveTemporaryAsync(
        Stream fileStream,
        string fileName,
        Uri requestBaseUri,
        CancellationToken ct);

    Task<string> PromoteTemporaryAsync(
        string imageUrl,
        CancellationToken ct);

    Task DeleteAsync(
        string? imageUrl,
        CancellationToken ct);

    Task CleanupTemporaryImagesAsync(
        TimeSpan maxAge,
        CancellationToken ct);
}
