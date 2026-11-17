using Esport.Application.Common;
using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Queries.Tournoi;

public record GetTournoisQuery(PaginationParams Pagination, int? JeuId)
    : IRequest<PagedResult<TournoiDto>>;

public class GetTournoisQueryHandler : IRequestHandler<GetTournoisQuery, PagedResult<TournoiDto>>
{
    private readonly IUnitOfWork _uow;
    public GetTournoisQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<TournoiDto>> Handle(GetTournoisQuery request, CancellationToken ct)
    {
        var result = await _uow.Tournois.GetPagedAsync(request.Pagination, request.JeuId, ct);
        return new PagedResult<TournoiDto>
        {
            Items = result.Items.Select(t => new TournoiDto
            {
                Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
                Dotation = t.Dotation, Statut = (int)t.Statut, JeuId = t.JeuId,
                NomJeu = t.Jeu?.Nom,
                NombreEquipesInscrites = t.Inscriptions.Count,
                NombreMatchs = t.Matchs.Count
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}

public record GetTournoiByIdQuery(int Id) : IRequest<TournoiDto>;

public class GetTournoiByIdQueryHandler : IRequestHandler<GetTournoiByIdQuery, TournoiDto>
{
    private readonly IUnitOfWork _uow;
    public GetTournoiByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TournoiDto> Handle(GetTournoiByIdQuery request, CancellationToken ct)
    {
        var t = await _uow.Tournois.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Tournoi {request.Id} introuvable.");

        return new TournoiDto
        {
            Id = t.Id, Nom = t.Nom, DateDebut = t.DateDebut, DateFin = t.DateFin,
            Dotation = t.Dotation, Statut = (int)t.Statut, JeuId = t.JeuId,
            NomJeu = t.Jeu?.Nom,
            NombreEquipesInscrites = t.Inscriptions.Count,
            NombreMatchs = t.Matchs.Count
        };
    }
}

public record GetInscriptionsByTournoiQuery(int TournoiId, PaginationParams Pagination)
    : IRequest<PagedResult<InscriptionDto>>;

public class GetInscriptionsByTournoiQueryHandler
    : IRequestHandler<GetInscriptionsByTournoiQuery, PagedResult<InscriptionDto>>
{
    private readonly IUnitOfWork _uow;
    public GetInscriptionsByTournoiQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<InscriptionDto>> Handle(GetInscriptionsByTournoiQuery request, CancellationToken ct)
    {
        var tournoi = await _uow.Tournois.GetByIdAsync(request.TournoiId, ct)
            ?? throw new NotFoundException($"Tournoi {request.TournoiId} introuvable.");

        var result = await _uow.Inscriptions.GetPagedByTournoiAsync(request.TournoiId, request.Pagination, ct);
        return new PagedResult<InscriptionDto>
        {
            Items = result.Items.Select(i => new InscriptionDto
            {
                Id = i.Id, DateInscription = i.DateInscription, TournoiId = i.TournoiId,
                EquipeId = i.EquipeId, NomTournoi = tournoi.Nom,
                NomEquipe = i.Equipe?.Nom, TagEquipe = i.Equipe?.Tag
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}

public record GetClassementQuery(int TournoiId) : IRequest<List<ClassementDto>>;

public class GetClassementQueryHandler : IRequestHandler<GetClassementQuery, List<ClassementDto>>
{
    private readonly IUnitOfWork _uow;
    public GetClassementQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<ClassementDto>> Handle(GetClassementQuery request, CancellationToken ct)
    {
        var tournoi = await _uow.Tournois.GetByIdAsync(request.TournoiId, ct)
            ?? throw new NotFoundException($"Tournoi {request.TournoiId} introuvable.");

        var inscriptions = await _uow.Inscriptions.GetByTournoiAsync(request.TournoiId, ct);
        var matchs = await _uow.Matchs.GetByTournoiTermineAsync(request.TournoiId, ct);

        var classements = inscriptions
            .Select(i => new ClassementDto
            {
                EquipeId = i.EquipeId,
                NomEquipe = i.Equipe?.Nom ?? "?",
                TagEquipe = i.Equipe?.Tag
            })
            .ToDictionary(c => c.EquipeId);

        foreach (var match in matchs)
        {
            if (match.ScoreEquipeA is null || match.ScoreEquipeB is null)
                continue;

            if (!classements.TryGetValue(match.EquipeAId, out var a))
            {
                a = new ClassementDto { EquipeId = match.EquipeAId, NomEquipe = match.EquipeA?.Nom ?? "?" };
                classements[match.EquipeAId] = a;
            }
            if (!classements.TryGetValue(match.EquipeBId, out var b))
            {
                b = new ClassementDto { EquipeId = match.EquipeBId, NomEquipe = match.EquipeB?.Nom ?? "?" };
                classements[match.EquipeBId] = b;
            }

            a.MatchsJoues++;
            b.MatchsJoues++;
            a.PointsMarques += match.ScoreEquipeA.Value;
            a.PointsEncaisses += match.ScoreEquipeB.Value;
            b.PointsMarques += match.ScoreEquipeB.Value;
            b.PointsEncaisses += match.ScoreEquipeA.Value;

            if (match.VainqueurId == match.EquipeAId)
            {
                a.Victoires++;
                a.Points += 3;
                b.Defaites++;
            }
            else if (match.VainqueurId == match.EquipeBId)
            {
                b.Victoires++;
                b.Points += 3;
                a.Defaites++;
            }
        }

        foreach (var c in classements.Values)
            c.DifferencePoints = c.PointsMarques - c.PointsEncaisses;

        var ordered = classements.Values
            .OrderByDescending(c => c.Points)
            .ThenByDescending(c => c.DifferencePoints)
            .ThenByDescending(c => c.PointsMarques)
            .ThenBy(c => c.NomEquipe)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
            ordered[i].Rang = i + 1;

        return ordered;
    }
}
