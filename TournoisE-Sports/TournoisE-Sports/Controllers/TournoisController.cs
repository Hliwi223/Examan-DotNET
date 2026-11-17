using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Tournoi;
using Esport.Application.DTOs;
using Esport.Application.Queries.Tournoi;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournoisController : ControllerBase
{
    private readonly IMediator _mediator;
    public TournoisController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? jeuId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTournoisQuery(new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }, jeuId), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var tournoi = await _mediator.Send(new GetTournoiByIdQuery(id), ct);
        return Ok(tournoi);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateTournoiDto dto, CancellationToken ct = default)
    {
        var tournoi = await _mediator.Send(new CreateTournoiCommand(dto), ct);
        return StatusCode(StatusCodes.Status201Created, tournoi);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTournoiDto dto, CancellationToken ct = default)
    {
        var tournoi = await _mediator.Send(new UpdateTournoiCommand(id, dto), ct);
        return Ok(tournoi);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteTournoiCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/classement")]
    public async Task<IActionResult> GetClassement(int id, CancellationToken ct = default)
    {
        var classement = await _mediator.Send(new GetClassementQuery(id), ct);
        return Ok(classement);
    }

    // --- Inscriptions (nested) ---
    [HttpGet("{id:int}/inscriptions")]
    public async Task<IActionResult> GetInscriptions(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInscriptionsByTournoiQuery(id, new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/inscriptions")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> CreateInscription(int id, [FromBody] CreateInscriptionDto dto, CancellationToken ct = default)
    {
        var inscription = await _mediator.Send(new CreateInscriptionCommand(id, dto), ct);
        return StatusCode(StatusCodes.Status201Created, inscription);
    }

    [HttpDelete("inscriptions/{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> DeleteInscription(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteInscriptionCommand(id), ct);
        return NoContent();
    }

    // --- Matchs (nested) ---
    [HttpGet("{id:int}/matchs")]
    public async Task<IActionResult> GetMatchs(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new Esport.Application.Queries.Match.GetMatchsByTournoiQuery(id, new Esport.Application.Common.PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }
}
