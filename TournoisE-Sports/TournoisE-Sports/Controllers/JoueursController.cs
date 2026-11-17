using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Joueur;
using Esport.Application.DTOs;
using Esport.Application.Queries.Joueur;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JoueursController : ControllerBase
{
    private readonly IMediator _mediator;
    public JoueursController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? equipeId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetJoueursQuery(new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }, equipeId), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var joueur = await _mediator.Send(new GetJoueurByIdQuery(id), ct);
        return Ok(joueur);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJoueurDto dto, CancellationToken ct = default)
    {
        var joueur = await _mediator.Send(new UpdateJoueurCommand(id, dto), ct);
        return Ok(joueur);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteJoueurCommand(id), ct);
        return NoContent();
    }
}
