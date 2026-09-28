using Asp.Versioning;

using Gym.Application.Common.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[Route("api/v{version:apiVersion}/images")]
[ApiVersion("1.0")]
[ApiController]
public class ImagesController(IImageStorage imageStorage) : ApiController
{
    [HttpPost("UploadImage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
}
