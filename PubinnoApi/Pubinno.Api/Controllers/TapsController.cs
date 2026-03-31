using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pubinno.Application.Taps.Queries;
using System;
using System.Threading.Tasks;

namespace Pubinno.Api.Controllers;

[ApiController]
[Route("v1/taps")]
public class TapsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TapsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{deviceId}/summary")]
    public async Task<IActionResult> GetSummary(string deviceId, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var query = new GetSummaryQuery(deviceId, from, to);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
