using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.Common;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Models;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JeuxController : ControllerBase
{
    private readonly IJeuRepository _jeux;

    public JeuxController(IJeuRepository jeux) => _jeux = jeux;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _jeux.GetPagedAsync(pagination, ct);
        return Ok(new
        {
            items = result.Items.Select(j => new JeuDto { Id = j.Id, Nom = j.Nom, Editeur = j.Editeur, Genre = j.Genre }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var jeu = await _jeux.GetByIdAsync(id, ct);
        if (jeu is null) return NotFound(new { message = $"Jeu {id} introuvable." });
        return Ok(new JeuDto { Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateJeuDto dto, CancellationToken ct)
    {
        if (await _jeux.ExistsAsync(dto.Nom, null, ct))
            return Conflict(new { message = $"Un jeu nommé '{dto.Nom}' existe déjà." });

        var jeu = new Jeu { Nom = dto.Nom, Editeur = dto.Editeur, Genre = dto.Genre };
        await _jeux.AddAsync(jeu, ct);
        return StatusCode(StatusCodes.Status201Created, new JeuDto { Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJeuDto dto, CancellationToken ct)
    {
        var jeu = await _jeux.GetByIdAsync(id, ct);
        if (jeu is null) return NotFound(new { message = $"Jeu {id} introuvable." });

        if (await _jeux.ExistsAsync(dto.Nom, id, ct))
            return Conflict(new { message = $"Un autre jeu nommé '{dto.Nom}' existe déjà." });

        jeu.Nom = dto.Nom;
        jeu.Editeur = dto.Editeur;
        jeu.Genre = dto.Genre;
        await _jeux.UpdateAsync(jeu, ct);
        return Ok(new JeuDto { Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var jeu = await _jeux.GetByIdAsync(id, ct);
        if (jeu is null) return NotFound(new { message = $"Jeu {id} introuvable." });
        try
        {
            await _jeux.DeleteAsync(jeu, ct);
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("REFERENCE") == true || ex.InnerException?.Message.Contains("constraint") == true)
        {
            return Conflict(new { message = "Impossible de supprimer : le jeu est référencé par un tournoi." });
        }
        return NoContent();
    }
}
