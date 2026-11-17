using Esport.Application.Common;
using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Queries.Joueur;

public record GetJoueursQuery(PaginationParams Pagination, int? EquipeId)
    : IRequest<PagedResult<JoueurDto>>;

public class GetJoueursQueryHandler : IRequestHandler<GetJoueursQuery, PagedResult<JoueurDto>>
{
    private readonly IUnitOfWork _uow;
    public GetJoueursQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<JoueurDto>> Handle(GetJoueursQuery request, CancellationToken ct)
    {
        var result = await _uow.Joueurs.GetPagedAsync(request.Pagination, request.EquipeId, ct);
        return new PagedResult<JoueurDto>
        {
            Items = result.Items.Select(j => new JoueurDto
            {
                Id = j.Id, Pseudo = j.Pseudo, Nom = j.Nom, Prenom = j.Prenom,
                Email = j.Email, EquipeId = j.EquipeId, NomEquipe = j.Equipe?.Nom
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}

public record GetJoueurByIdQuery(int Id) : IRequest<JoueurDto>;

public class GetJoueurByIdQueryHandler : IRequestHandler<GetJoueurByIdQuery, JoueurDto>
{
    private readonly IUnitOfWork _uow;
    public GetJoueurByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JoueurDto> Handle(GetJoueurByIdQuery request, CancellationToken ct)
    {
        var joueur = await _uow.Joueurs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Joueur {request.Id} introuvable.");

        return new JoueurDto
        {
            Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom,
            Prenom = joueur.Prenom, Email = joueur.Email,
            EquipeId = joueur.EquipeId, NomEquipe = joueur.Equipe?.Nom
        };
    }
}
