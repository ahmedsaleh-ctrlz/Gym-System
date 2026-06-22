using Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[Route("api/v{version:apiVersion}/images")]
[ApiVersion("1.0")]
[ApiController]
public class ImagesController : ApiController
{
    [HttpPost("UploadImage")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Add Image.")]
    [EndpointDescription("Add Image to Member/Coaches and Return url.")]
    [EndpointName("UploadImage")]
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
            return BadRequest("No file");
        }

        var dir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads");

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var fileName =
            $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        var fullPath =
            Path.Combine(dir, fileName);

        await using var stream =
            new FileStream(fullPath, FileMode.Create);

        await file.CopyToAsync(stream, ct);

        var imageUrl =
            $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}";

        return Ok(new
        {
            path = imageUrl
        });
    }
}