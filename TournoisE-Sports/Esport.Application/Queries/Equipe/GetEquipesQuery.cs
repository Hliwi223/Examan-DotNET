using Esport.Application.Common;
using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Queries.Equipe;

public record GetEquipesQuery(PaginationParams Pagination) : IRequest<PagedResult<EquipeDto>>;

public class GetEquipesQueryHandler : IRequestHandler<GetEquipesQuery, PagedResult<EquipeDto>>
{
    private readonly IUnitOfWork _uow;
    public GetEquipesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<EquipeDto>> Handle(GetEquipesQuery request, CancellationToken ct)
    {
        var result = await _uow.Equipes.GetPagedAsync(request.Pagination, ct);
        return new PagedResult<EquipeDto>
        {
            Items = result.Items.Select(e => new EquipeDto
            {
                Id = e.Id, Nom = e.Nom, Tag = e.Tag,
                DateCreation = e.DateCreation, NombreJoueurs = e.Joueurs?.Count ?? 0
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}

public record GetEquipeByIdQuery(int Id) : IRequest<EquipeDto>;

public class GetEquipeByIdQueryHandler : IRequestHandler<GetEquipeByIdQuery, EquipeDto>
{
    private readonly IUnitOfWork _uow;
    public GetEquipeByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EquipeDto> Handle(GetEquipeByIdQuery request, CancellationToken ct)
    {
        var equipe = await _uow.Equipes.GetByIdWithJoueursAsync(request.Id, ct)
            ?? throw new NotFoundException($"Equipe {request.Id} introuvable.");

        return new EquipeDto
        {
            Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag,
            DateCreation = equipe.DateCreation, NombreJoueurs = equipe.Joueurs?.Count ?? 0
        };
    }
}

public record GetJoueursByEquipeQuery(int EquipeId, PaginationParams Pagination)
    : IRequest<PagedResult<JoueurDto>>;

public class GetJoueursByEquipeQueryHandler : IRequestHandler<GetJoueursByEquipeQuery, PagedResult<JoueurDto>>
{
    private readonly IUnitOfWork _uow;
    public GetJoueursByEquipeQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<JoueurDto>> Handle(GetJoueursByEquipeQuery request, CancellationToken ct)
    {
        var equipe = await _uow.Equipes.GetByIdAsync(request.EquipeId, ct)
            ?? throw new NotFoundException($"Equipe {request.EquipeId} introuvable.");

        var result = await _uow.Joueurs.GetPagedAsync(request.Pagination, request.EquipeId, ct);
        return new PagedResult<JoueurDto>
        {
            Items = result.Items.Select(j => new JoueurDto
            {
                Id = j.Id, Pseudo = j.Pseudo, Nom = j.Nom, Prenom = j.Prenom,
                Email = j.Email, EquipeId = j.EquipeId, NomEquipe = equipe.Nom
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}
