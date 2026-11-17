using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.Common;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JoueursController : ControllerBase
{
    private readonly IJoueurRepository _joueurs;
    private readonly IEquipeRepository _equipes;

    public JoueursController(IJoueurRepository joueurs, IEquipeRepository equipes)
    {
        _joueurs = joueurs;
        _equipes = equipes;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, [FromQuery] int? equipeId, CancellationToken ct)
    {
        var result = await _joueurs.GetPagedAsync(pagination, equipeId, ct);
        return Ok(new
        {
            items = result.Items.Select(j => new JoueurDto { Id = j.Id, Pseudo = j.Pseudo, Nom = j.Nom, Prenom = j.Prenom, Email = j.Email, EquipeId = j.EquipeId, NomEquipe = j.Equipe?.Nom }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var joueur = await _joueurs.GetByIdAsync(id, ct);
        if (joueur is null) return NotFound(new { message = $"Joueur {id} introuvable." });
        return Ok(new JoueurDto { Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom, Prenom = joueur.Prenom, Email = joueur.Email, EquipeId = joueur.EquipeId, NomEquipe = joueur.Equipe?.Nom });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJoueurDto dto, CancellationToken ct)
    {
        var joueur = await _joueurs.GetByIdAsync(id, ct);
        if (joueur is null) return NotFound(new { message = $"Joueur {id} introuvable." });

        var equipe = await _equipes.GetByIdAsync(dto.EquipeId, ct);
        if (equipe is null) return BadRequest(new { message = $"Equipe {dto.EquipeId} introuvable." });

        joueur.Pseudo = dto.Pseudo;
        joueur.Nom = dto.Nom;
        joueur.Prenom = dto.Prenom;
        joueur.Email = dto.Email;
        joueur.EquipeId = dto.EquipeId;
        await _joueurs.UpdateAsync(joueur, ct);
        return Ok(new JoueurDto { Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom, Prenom = joueur.Prenom, Email = joueur.Email, EquipeId = joueur.EquipeId, NomEquipe = equipe.Nom });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var joueur = await _joueurs.GetByIdAsync(id, ct);
        if (joueur is null) return NotFound(new { message = $"Joueur {id} introuvable." });
        await _joueurs.DeleteAsync(joueur, ct);
        return NoContent();
    }
}
