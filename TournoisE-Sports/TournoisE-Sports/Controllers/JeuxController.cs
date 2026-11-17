using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Jeu;
using Esport.Application.DTOs;
using Esport.Application.Queries.Jeu;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JeuxController : ControllerBase
{
    private readonly IMediator _mediator;
    public JeuxController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetJeusQuery(new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var jeu = await _mediator.Send(new GetJeuByIdQuery(id), ct);
        return Ok(jeu);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateJeuDto dto, CancellationToken ct = default)
    {
        var jeu = await _mediator.Send(new CreateJeuCommand(dto), ct);
        return StatusCode(StatusCodes.Status201Created, jeu);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJeuDto dto, CancellationToken ct = default)
    {
        var jeu = await _mediator.Send(new UpdateJeuCommand(id, dto), ct);
        return Ok(jeu);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteJeuCommand(id), ct);
        return NoContent();
    }
}
