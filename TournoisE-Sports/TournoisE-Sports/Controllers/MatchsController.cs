using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Match;
using Esport.Application.DTOs;
using Esport.Application.Queries.Match;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MatchsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var match = await _mediator.Send(new GetMatchByIdQuery(id), ct);
        return Ok(match);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateMatchDto dto, CancellationToken ct = default)
    {
        var match = await _mediator.Send(new CreateMatchCommand(dto), ct);
        return StatusCode(StatusCodes.Status201Created, match);
    }

    [HttpPut("{id:int}/score")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreDto dto, CancellationToken ct = default)
    {
        var match = await _mediator.Send(new UpdateScoreCommand(id, dto), ct);
        return Ok(match);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteMatchCommand(id), ct);
        return NoContent();
    }
}
