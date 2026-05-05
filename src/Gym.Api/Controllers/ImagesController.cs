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

    private async Task<IActionResult> HandleImage(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file");

        var Dir = @"D:\Uploads";



        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        var path = Path.Combine(Dir, fileName);

        if (!Directory.Exists(Dir))
        {
            Directory.CreateDirectory(Dir);
        }

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, ct);



        return Ok(new { path });
    }
}
