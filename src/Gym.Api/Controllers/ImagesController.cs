using Asp.Versioning;

using Gym.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gym.Api.Controllers;

[Route("api/v{version:apiVersion}/images")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
[EnableRateLimiting("HeavyOperations")]
public class ImagesController(IImageStorage imageStorage) : ApiController
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    [HttpPost("UploadImage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add Image.")]
    [EndpointDescription("Add Image to Member/Coaches and Return url.")]
    [EndpointName("UploadImage")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        return await HandleImage(file, ct);
    }

    private async Task<IActionResult> HandleImage(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid file",
                Detail = "No file was uploaded or file is empty.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "File size limit exceeded",
                Detail = "Maximum allowed file size is 5 MB.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var extension = System.IO.Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid file extension",
                Detail = "Only .jpg, .jpeg, .png, and .webp file formats are supported.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var header = new byte[8];
        await using (var readStream = file.OpenReadStream())
        {
            var read = await readStream.ReadAsync(header.AsMemory(0, header.Length), ct);
            if (!IsValidImageSignature(extension, header, read))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid file signature",
                    Detail = "File content does not match a valid image signature.",
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }

        var requestBaseUri = new Uri($"{Request.Scheme}://{Request.Host}/");
        await using var stream = file.OpenReadStream();

        var imageUrl = await imageStorage.SaveTemporaryAsync(
            stream,
            file.FileName,
            requestBaseUri,
            ct);

        return Ok(new
        {
            path = imageUrl
        });
    }

    private static bool IsValidImageSignature(string extension, byte[] header, int bytesRead)
    {
        if (bytesRead < 4)
        {
            return false;
        }

        var ext = extension.ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".webp" => bytesRead >= 4 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46,
            _ => false
        };
    }
}
