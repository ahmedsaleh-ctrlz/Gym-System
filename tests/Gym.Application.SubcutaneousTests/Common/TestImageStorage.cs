using Gym.Application.Common.Interfaces;

namespace Gym.Application.SubcutaneousTests.Common;

public sealed class TestImageStorage : IImageStorage
{
    public Task<string> SaveTemporaryAsync(
        Stream fileStream,
        string fileName,
        Uri requestBaseUri,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var extension = Path.GetExtension(fileName);
        var storedFileName = $"test-{Guid.NewGuid()}{extension}";

        return Task.FromResult(
            $"Uploads/temp/{storedFileName}");
    }

    public Task<string> PromoteTemporaryAsync(
        string imageUrl,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.FromResult(imageUrl);

        var fileName = Path.GetFileName(imageUrl);

        return Task.FromResult(
            $"Uploads/{fileName}");
    }

    public Task DeleteAsync(
        string? imageUrl,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    public Task CleanupTemporaryImagesAsync(
        TimeSpan maxAge,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}