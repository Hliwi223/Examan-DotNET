using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Models;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchsController : ControllerBase
{
    private readonly IMatchRepository _matchs;
    private readonly ITournoiRepository _tournois;
    private readonly IEquipeRepository _equipes;
    private readonly IInscriptionRepository _inscriptions;

    public MatchsController(IMatchRepository matchs, ITournoiRepository tournois, IEquipeRepository equipes, IInscriptionRepository inscriptions)
    {
        _matchs = matchs;
        _tournois = tournois;
        _equipes = equipes;
        _inscriptions = inscriptions;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var m = await _matchs.GetByIdAsync(id, ct);
        if (m is null) return NotFound(new { message = $"Match {id} introuvable." });
        return Ok(ToDto(m));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateMatchDto dto, CancellationToken ct)
    {
        // Règle : pas de match contre soi-même
        if (dto.EquipeAId == dto.EquipeBId)
            return BadRequest(new { message = "Une équipe ne peut pas jouer contre elle-même." });

        var tournoi = await _tournois.GetByIdAsync(dto.TournoiId, ct);
        if (tournoi is null)
            return BadRequest(new { message = $"Tournoi {dto.TournoiId} introuvable." });

        var equipeA = await _equipes.GetByIdAsync(dto.EquipeAId, ct);
        if (equipeA is null)
            return BadRequest(new { message = $"Equipe {dto.EquipeAId} introuvable." });
        var equipeB = await _equipes.GetByIdAsync(dto.EquipeBId, ct);
        if (equipeB is null)
            return BadRequest(new { message = $"Equipe {dto.EquipeBId} introuvable." });

        // Règle : les deux équipes doivent être inscrites au tournoi
        var inscA = await _inscriptions.GetByTournoiEtEquipeAsync(dto.TournoiId, dto.EquipeAId, ct);
        var inscB = await _inscriptions.GetByTournoiEtEquipeAsync(dto.TournoiId, dto.EquipeBId, ct);
        if (inscA is null || inscB is null)
            return BadRequest(new { message = "Les deux équipes doivent être inscrites au tournoi." });

        var match = new Match
        {
            DateMatch = dto.DateMatch,
            TournoiId = dto.TournoiId,
            EquipeAId = dto.EquipeAId,
            EquipeBId = dto.EquipeBId,
            Termine = false
        };
        await _matchs.AddAsync(match, ct);
        return StatusCode(StatusCodes.Status201Created, ToDto(match, tournoi.Nom, equipeA.Nom, equipeB.Nom));
    }

    [HttpPut("{id:int}/score")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreDto dto, CancellationToken ct)
    {
        var match = await _matchs.GetByIdAsync(id, ct);
        if (match is null) return NotFound(new { message = $"Match {id} introuvable." });

        // Égalité refusée : il doit y avoir un vainqueur
        if (dto.ScoreEquipeA == dto.ScoreEquipeB)
            return BadRequest(new { message = "Le score ne peut pas être nul : il doit y avoir un vainqueur (égalité refusée)." });

        match.ScoreEquipeA = dto.ScoreEquipeA;
        match.ScoreEquipeB = dto.ScoreEquipeB;
        match.Termine = true;
        match.VainqueurId = dto.ScoreEquipeA > dto.ScoreEquipeB ? match.EquipeAId : match.EquipeBId;

        await _matchs.UpdateAsync(match, ct);
        return Ok(ToDto(match));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var match = await _matchs.GetByIdAsync(id, ct);
        if (match is null) return NotFound(new { message = $"Match {id} introuvable." });
        await _matchs.DeleteAsync(match, ct);
        return NoContent();
    }

    private static MatchDto ToDto(Match m, string? nomTournoi = null, string? nomA = null, string? nomB = null)
    {
        return new MatchDto
        {
            Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
            Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = nomTournoi ?? m.Tournoi?.Nom,
            EquipeAId = m.EquipeAId, NomEquipeA = nomA ?? m.EquipeA?.Nom,
            EquipeBId = m.EquipeBId, NomEquipeB = nomB ?? m.EquipeB?.Nom,
            VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
        };
    }
}
