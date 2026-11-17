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
public class EquipesController : ControllerBase
{
    private readonly IEquipeRepository _equipes;
    private readonly IJoueurRepository _joueurs;

    public EquipesController(IEquipeRepository equipes, IJoueurRepository joueurs)
    {
        _equipes = equipes;
        _joueurs = joueurs;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _equipes.GetPagedAsync(pagination, ct);
        return Ok(new
        {
            items = result.Items.Select(e => new EquipeDto { Id = e.Id, Nom = e.Nom, Tag = e.Tag, DateCreation = e.DateCreation, NombreJoueurs = e.Joueurs.Count }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var equipe = await _equipes.GetByIdWithJoueursAsync(id, ct);
        if (equipe is null) return NotFound(new { message = $"Equipe {id} introuvable." });
        return Ok(new EquipeDto { Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag, DateCreation = equipe.DateCreation, NombreJoueurs = equipe.Joueurs.Count });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateEquipeDto dto, CancellationToken ct)
    {
        var equipe = new Equipe { Nom = dto.Nom, Tag = dto.Tag, DateCreation = DateTime.UtcNow };
        await _equipes.AddAsync(equipe, ct);
        return StatusCode(StatusCodes.Status201Created, new EquipeDto { Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag, DateCreation = equipe.DateCreation });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipeDto dto, CancellationToken ct)
    {
        var equipe = await _equipes.GetByIdAsync(id, ct);
        if (equipe is null) return NotFound(new { message = $"Equipe {id} introuvable." });
        equipe.Nom = dto.Nom;
        equipe.Tag = dto.Tag;
        await _equipes.UpdateAsync(equipe, ct);
        return Ok(new EquipeDto { Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag, DateCreation = equipe.DateCreation });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var equipe = await _equipes.GetByIdAsync(id, ct);
        if (equipe is null) return NotFound(new { message = $"Equipe {id} introuvable." });
        try
        {
            await _equipes.DeleteAsync(equipe, ct);
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("REFERENCE") == true || ex.InnerException?.Message.Contains("constraint") == true)
        {
            return Conflict(new { message = "Impossible de supprimer : l'équipe est liée à des inscriptions, joueurs ou matchs." });
        }
        return NoContent();
    }

    [HttpGet("{id:int}/joueurs")]
    public async Task<IActionResult> GetJoueurs(int id, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var equipe = await _equipes.GetByIdAsync(id, ct);
        if (equipe is null) return NotFound(new { message = $"Equipe {id} introuvable." });

        var result = await _joueurs.GetPagedAsync(pagination, id, ct);
        return Ok(new
        {
            items = result.Items.Select(j => new JoueurDto { Id = j.Id, Pseudo = j.Pseudo, Nom = j.Nom, Prenom = j.Prenom, Email = j.Email, EquipeId = j.EquipeId, NomEquipe = j.Equipe?.Nom }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpPost("{id:int}/joueurs")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> AddJoueur(int id, [FromBody] CreateJoueurDto dto, CancellationToken ct)
    {
        var equipe = await _equipes.GetByIdAsync(id, ct);
        if (equipe is null) return NotFound(new { message = $"Equipe {id} introuvable." });

        var joueur = new Joueur
        {
            Pseudo = dto.Pseudo,
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Email = dto.Email,
            EquipeId = id
        };
        await _joueurs.AddAsync(joueur, ct);
        return StatusCode(StatusCodes.Status201Created, new JoueurDto { Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom, Prenom = joueur.Prenom, Email = joueur.Email, EquipeId = joueur.EquipeId, NomEquipe = equipe.Nom });
    }
}
