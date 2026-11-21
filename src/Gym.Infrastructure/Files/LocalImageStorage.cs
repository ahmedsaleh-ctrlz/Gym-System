using Gym.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Gym.Infrastructure.Files;

public sealed class LocalImageStorage : IImageStorage
{
    private const string UploadsSegment = "Uploads";
    private const string TempSegment = "temp";

    private readonly string _uploadsRoot;
    private readonly string _tempRoot;

    public LocalImageStorage(IConfiguration? configuration = null)
    {
        var configuredPath = configuration?["Storage:UploadsPath"]
            ?? Environment.GetEnvironmentVariable("UPLOADS_STORAGE_PATH");

        _uploadsRoot = !string.IsNullOrWhiteSpace(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), UploadsSegment);

        _tempRoot = Path.Combine(_uploadsRoot, TempSegment);

        Directory.CreateDirectory(_uploadsRoot);
        Directory.CreateDirectory(_tempRoot);
    }

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public async Task<string> SaveTemporaryAsync(
        Stream fileStream,
        string fileName,
        Uri requestBaseUri,
        CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Unsupported image format. Allowed formats: .jpg, .jpeg, .png, .webp", nameof(fileName));
        }

        Directory.CreateDirectory(_tempRoot);

        var storedFileName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(_tempRoot, storedFileName);

        await using var output = new FileStream(fullPath, FileMode.CreateNew);
        await fileStream.CopyToAsync(output, ct);

        return $"{UploadsSegment}/{TempSegment}/{storedFileName}";
    }

    public Task<string> PromoteTemporaryAsync(
        string imageUrl,
        CancellationToken ct)
    {
        if (!TryGetUploadsRelativePath(imageUrl, out var relativePath) ||
            !IsTemporaryPath(relativePath))
        {
            return Task.FromResult(imageUrl);
        }

        ct.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_uploadsRoot);

        var fileName = Path.GetFileName(relativePath);
        var sourcePath = Path.Combine(_uploadsRoot, TempSegment, fileName);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Temporary image was not found.", sourcePath);
        }

        var destinationPath = GetAvailablePermanentPath(fileName);
        File.Move(sourcePath, destinationPath);

        var promotedFileName = Path.GetFileName(destinationPath);
        return Task.FromResult(BuildUploadsUrl(imageUrl, promotedFileName));
    }

    public Task DeleteAsync(
        string? imageUrl,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) ||
            !TryGetUploadsRelativePath(imageUrl, out var relativePath))
        {
            return Task.CompletedTask;
        }

        ct.ThrowIfCancellationRequested();

        var fullPath = Path.Combine(
        _uploadsRoot,
        relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task CleanupTemporaryImagesAsync(
        TimeSpan maxAge,
        CancellationToken ct)
    {
        if (!Directory.Exists(_tempRoot))
        {
            return Task.CompletedTask;
        }

        var cutoffUtc = DateTime.UtcNow.Subtract(maxAge);

        foreach (var file in Directory.EnumerateFiles(_tempRoot))
        {
            ct.ThrowIfCancellationRequested();

            if (File.GetCreationTimeUtc(file) <= cutoffUtc)
            {
                File.Delete(file);
            }
        }

        return Task.CompletedTask;
    }

    private static bool TryGetUploadsRelativePath(
        string imageUrl,
        out string relativePath)
    {
        relativePath = string.Empty;

        var path = imageUrl;
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            path = uri.LocalPath;
        }

        path = path.Replace('\\', '/').TrimStart('/');
        var uploadsIndex = path.IndexOf($"{UploadsSegment}/", StringComparison.OrdinalIgnoreCase);

        if (uploadsIndex < 0)
        {
            return false;
        }

        relativePath = path[(uploadsIndex + UploadsSegment.Length + 1)..];
        return !string.IsNullOrWhiteSpace(relativePath) &&
            !relativePath.Contains("..", StringComparison.Ordinal);
    }

    private static bool IsTemporaryPath(string relativePath)
    {
        return relativePath.StartsWith($"{TempSegment}/", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildUploadsUrl(
        string originalUrl,
        string fileName)
    {
        return $"{UploadsSegment}/{fileName}";
    }

    private string GetAvailablePermanentPath(string fileName)
    {
        var destinationPath = Path.Combine(_uploadsRoot, fileName);

        if (!File.Exists(destinationPath))
        {
            return destinationPath;
        }

        var extension = Path.GetExtension(fileName);
        return Path.Combine(_uploadsRoot, $"{Guid.NewGuid()}{extension}");
    }
}
