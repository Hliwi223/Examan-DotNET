using Esport.Application.Common;
using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Queries.Match;

public record GetMatchByIdQuery(int Id) : IRequest<MatchDto>;

public class GetMatchByIdQueryHandler : IRequestHandler<GetMatchByIdQuery, MatchDto>
{
    private readonly IUnitOfWork _uow;
    public GetMatchByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<MatchDto> Handle(GetMatchByIdQuery request, CancellationToken ct)
    {
        var m = await _uow.Matchs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Match {request.Id} introuvable.");

        return new MatchDto
        {
            Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
            Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = m.Tournoi?.Nom,
            EquipeAId = m.EquipeAId, NomEquipeA = m.EquipeA?.Nom,
            EquipeBId = m.EquipeBId, NomEquipeB = m.EquipeB?.Nom,
            VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
        };
    }
}

public record GetMatchsByTournoiQuery(int TournoiId, PaginationParams Pagination)
    : IRequest<PagedResult<MatchDto>>;

public class GetMatchsByTournoiQueryHandler : IRequestHandler<GetMatchsByTournoiQuery, PagedResult<MatchDto>>
{
    private readonly IUnitOfWork _uow;
    public GetMatchsByTournoiQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<MatchDto>> Handle(GetMatchsByTournoiQuery request, CancellationToken ct)
    {
        var tournoi = await _uow.Tournois.GetByIdAsync(request.TournoiId, ct)
            ?? throw new NotFoundException($"Tournoi {request.TournoiId} introuvable.");

        var result = await _uow.Matchs.GetPagedByTournoiAsync(request.TournoiId, request.Pagination, ct);
        return new PagedResult<MatchDto>
        {
            Items = result.Items.Select(m => new MatchDto
            {
                Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
                Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = m.Tournoi?.Nom,
                EquipeAId = m.EquipeAId, NomEquipeA = m.EquipeA?.Nom,
                EquipeBId = m.EquipeBId, NomEquipeB = m.EquipeB?.Nom,
                VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
