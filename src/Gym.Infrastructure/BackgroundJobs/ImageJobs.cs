using Gym.Application.Common.Interfaces;

namespace Gym.Infrastructure.BackgroundJobs;

public sealed class ImageJobs(IImageStorage imageStorage)
{
    public async Task CleanupTemporaryImages()
    {
        await imageStorage.CleanupTemporaryImagesAsync(
            TimeSpan.FromHours(24),
            CancellationToken.None);
    }
}
