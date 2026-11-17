using Esport.Application.Common;
using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Queries.Jeu;

/// <summary>Liste pagin\u00e9e des jeux (Query c\u00f4t\u00e9 lecture).</summary>
public record GetJeusQuery(PaginationParams Pagination) : IRequest<PagedResult<JeuDto>>;

public class GetJeusQueryHandler : IRequestHandler<GetJeusQuery, PagedResult<JeuDto>>
{
    private readonly IUnitOfWork _uow;
    public GetJeusQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<JeuDto>> Handle(GetJeusQuery request, CancellationToken ct)
    {
        var result = await _uow.Jeux.GetPagedAsync(request.Pagination, ct);
        return new PagedResult<JeuDto>
        {
            Items = result.Items.Select(j => new JeuDto
            {
                Id = j.Id, Nom = j.Nom, Editeur = j.Editeur, Genre = j.Genre
            }).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }
}

/// <summary>D\u00e9tail d'un jeu par identifiant.</summary>
public record GetJeuByIdQuery(int Id) : IRequest<JeuDto>;

public class GetJeuByIdQueryHandler : IRequestHandler<GetJeuByIdQuery, JeuDto>
{
    private readonly IUnitOfWork _uow;
    public GetJeuByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JeuDto> Handle(GetJeuByIdQuery request, CancellationToken ct)
    {
        var jeu = await _uow.Jeux.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Jeu {request.Id} introuvable.");

        return new JeuDto
        {
            Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre
        };
    }
}
