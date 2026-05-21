using Asp.Versioning;
using Gym.Application.Features.Dashboard.Dtos;
using Gym.Application.Features.Dashboard.Queries.GetAdminDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/dashboard")]
    public class DashBoardController(ISender sender) : ApiController
    {
        [HttpGet("stats")]
        [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await sender.Send(new GetAdminDashboardQuery(), ct);

            return result.Match(
                response => Ok(response),
                Problem);
        }

    }
}
