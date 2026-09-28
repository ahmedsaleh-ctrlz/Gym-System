using Gym.Application.Features.Invoices.Queries.GetInvoiceByPaymentId;
using Gym.Application.Features.Invoices.Queries.GetInvoicePdf;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class InvoicesController(ISender sender) : ApiController
{
    [HttpGet("{id:int}", Name = "GetInvoiceByPaymentId")]
    [EndpointName("GetInvoiceByPaymentId")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetInvoiceByPaymentIdQuery(id),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet(
    "{id:int}/pdf",
    Name = "DownloadInvoicePdf")]
    [EndpointName("DownloadInvoicePdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf(
    int id,
    CancellationToken ct)
    {
        var result = await sender.Send(
            new GetInvoicePdfQuery(id),
            ct);

        return result.Match(
            pdf => File(
                pdf,
                "application/pdf",
                $"Invoice-{id}.pdf"),
            Problem);
    }
}