using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Equipe;
using Esport.Application.Commands.Joueur;
using Esport.Application.DTOs;
using Esport.Application.Queries.Equipe;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipesController : ControllerBase
{
    private readonly IMediator _mediator;
    public EquipesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEquipesQuery(new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var equipe = await _mediator.Send(new GetEquipeByIdQuery(id), ct);
        return Ok(equipe);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateEquipeDto dto, CancellationToken ct = default)
    {
        var equipe = await _mediator.Send(new CreateEquipeCommand(dto), ct);
        return StatusCode(StatusCodes.Status201Created, equipe);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipeDto dto, CancellationToken ct = default)
    {
        var equipe = await _mediator.Send(new UpdateEquipeCommand(id, dto), ct);
        return Ok(equipe);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteEquipeCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/joueurs")]
    public async Task<IActionResult> GetJoueurs(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetJoueursByEquipeQuery(id, new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/joueurs")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> AddJoueur(int id, [FromBody] CreateJoueurDto dto, CancellationToken ct = default)
    {
        dto.EquipeId = id;
        var joueur = await _mediator.Send(new CreateJoueurCommand(dto), ct);
        return StatusCode(StatusCodes.Status201Created, joueur);
    }
}
