using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.Common;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Models;
using TournoisE_Sports.Repositories.Interfaces;
using TournoisE_Sports.Services;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournoisController : ControllerBase
{
    private readonly ITournoiRepository _tournois;
    private readonly IJeuRepository _jeux;
    private readonly IInscriptionRepository _inscriptions;
    private readonly IMatchRepository _matchs;
    private readonly IRankingService _ranking;

    public TournoisController(
        ITournoiRepository tournois,
        IJeuRepository jeux,
        IInscriptionRepository inscriptions,
        IMatchRepository matchs,
        IRankingService ranking)
    {
        _tournois = tournois;
        _jeux = jeux;
        _inscriptions = inscriptions;
        _matchs = matchs;
        _ranking = ranking;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, [FromQuery] int? jeuId, CancellationToken ct)
    {
        var result = await _tournois.GetPagedAsync(pagination, jeuId, ct);
        return Ok(new
        {
            items = result.Items.Select(t => new TournoiDto
            {
                Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
                Dotation = t.Dotation, Statut = t.Statut, JeuId = t.JeuId, NomJeu = t.Jeu?.Nom,
                NombreEquipesInscrites = t.Inscriptions.Count, NombreMatchs = t.Matchs.Count
            }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });
        return Ok(new TournoiDto
        {
            Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
            Dotation = t.Dotation, Statut = t.Statut, JeuId = t.JeuId, NomJeu = t.Jeu?.Nom,
            NombreEquipesInscrites = t.Inscriptions.Count, NombreMatchs = t.Matchs.Count
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Create([FromBody] CreateTournoiDto dto, CancellationToken ct)
    {
        if (dto.DateFin < dto.DateDebut)
            return BadRequest(new { message = "DateFin doit être postérieure à DateDebut." });

        var jeu = await _jeux.GetByIdAsync(dto.JeuId, ct);
        if (jeu is null) return BadRequest(new { message = $"Jeu {dto.JeuId} introuvable." });

        var t = new Tournoi
        {
            Nom = dto.Nom, DateDebut = dto.DateDebut, DateFin = dto.DateFin,
            Dotation = dto.Dotation, Statut = dto.Statut, JeuId = dto.JeuId
        };
        await _tournois.AddAsync(t, ct);
        return StatusCode(StatusCodes.Status201Created, new TournoiDto
        {
            Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
            Dotation = t.Dotation, Statut = t.Statut, JeuId = t.JeuId, NomJeu = jeu.Nom
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTournoiDto dto, CancellationToken ct)
    {
        if (dto.DateFin < dto.DateDebut)
            return BadRequest(new { message = "DateFin doit être postérieure à DateDebut." });

        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });

        var jeu = await _jeux.GetByIdAsync(dto.JeuId, ct);
        if (jeu is null) return BadRequest(new { message = $"Jeu {dto.JeuId} introuvable." });

        t.Nom = dto.Nom;
        t.DateDebut = dto.DateDebut;
        t.DateFin = dto.DateFin;
        t.Dotation = dto.Dotation;
        t.Statut = dto.Statut;
        t.JeuId = dto.JeuId;
        await _tournois.UpdateAsync(t, ct);
        return Ok(new TournoiDto
        {
            Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
            Dotation = t.Dotation, Statut = t.Statut, JeuId = t.JeuId, NomJeu = jeu.Nom
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });
        // Cascade Inscriptions, mais Matchs Restrict → vérifier
        var matchs = await _matchs.GetByTournoiAsync(id, ct);
        if (matchs.Any())
            return Conflict(new { message = "Impossible de supprimer : ce tournoi contient des matchs. Supprimez d'abord les matchs." });
        await _tournois.DeleteAsync(t, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/classement")]
    public async Task<IActionResult> GetClassement(int id, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });
        var classement = await _ranking.GetClassementAsync(id, ct);
        return Ok(classement);
    }

    // --- Inscriptions (nested) ---
    [HttpGet("{id:int}/inscriptions")]
    public async Task<IActionResult> GetInscriptions(int id, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });
        var result = await _inscriptions.GetPagedByTournoiAsync(id, pagination, ct);
        return Ok(new
        {
            items = result.Items.Select(i => new InscriptionDto
            {
                Id = i.Id, DateInscription = i.DateInscription, TournoiId = i.TournoiId,
                NomTournoi = i.Tournoi?.Nom, EquipeId = i.EquipeId, NomEquipe = i.Equipe?.Nom, TagEquipe = i.Equipe?.Tag
            }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    [HttpPost("{id:int}/inscriptions")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> CreateInscription(int id, [FromBody] CreateInscriptionDto dto, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });

        var exist = await _inscriptions.GetByTournoiEtEquipeAsync(id, dto.EquipeId, ct);
        if (exist is not null)
            return Conflict(new { message = "Cette équipe est déjà inscrite à ce tournoi." });

        var inscription = new Inscription { TournoiId = id, EquipeId = dto.EquipeId, DateInscription = DateTime.UtcNow };
        try
        {
            await _inscriptions.AddAsync(inscription, ct);
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true || ex.InnerException?.Message.Contains("duplicate") == true || ex.InnerException?.Message.Contains("IX_Inscriptions") == true)
        {
            return Conflict(new { message = "Cette équipe est déjà inscrite à ce tournoi." });
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true || ex.InnerException?.Message.Contains("REFERENCE") == true)
        {
            return BadRequest(new { message = $"Equipe {dto.EquipeId} introuvable." });
        }

        var saved = await _inscriptions.GetByTournoiEtEquipeAsync(id, dto.EquipeId, ct);
        return StatusCode(StatusCodes.Status201Created, new InscriptionDto
        {
            Id = inscription.Id, DateInscription = inscription.DateInscription, TournoiId = id,
            NomTournoi = t.Nom, EquipeId = dto.EquipeId, NomEquipe = saved?.Equipe?.Nom
        });
    }

    // --- Matchs (nested) ---
    [HttpGet("{id:int}/matchs")]
    public async Task<IActionResult> GetMatchs(int id, [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var t = await _tournois.GetByIdAsync(id, ct);
        if (t is null) return NotFound(new { message = $"Tournoi {id} introuvable." });
        var result = await _matchs.GetPagedByTournoiAsync(id, pagination, ct);
        return Ok(new
        {
            items = result.Items.Select(m => new MatchDto
            {
                Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
                Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = m.Tournoi?.Nom,
                EquipeAId = m.EquipeAId, NomEquipeA = m.EquipeA?.Nom,
                EquipeBId = m.EquipeBId, NomEquipeB = m.EquipeB?.Nom,
                VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
            }),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }
}
