using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pubinno.Application.Pours.Commands;
using System.Threading.Tasks;

namespace Pubinno.Api.Controllers;

[ApiController]
[Route("v1/pours")]
public class PoursController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoursController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePour([FromBody] CreatePourCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok();
    }
}
