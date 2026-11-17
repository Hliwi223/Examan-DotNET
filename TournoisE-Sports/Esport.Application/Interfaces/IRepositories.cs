using Esport.Application.Common;
using Esport.Domain.Entities;

namespace Esport.Application.Interfaces;

public interface IJeuRepository : IRepository<Jeu>
{
    Task<bool> ExistsAsync(string nom, int? excludeId = null, CancellationToken ct = default);
}

public interface IEquipeRepository : IRepository<Equipe>
{
    Task<Equipe?> GetByIdWithJoueursAsync(int id, CancellationToken ct = default);
}

public interface IJoueurRepository : IRepository<Joueur>
{
    Task<PagedResult<Joueur>> GetPagedAsync(PaginationParams pagination, int? equipeId = null, CancellationToken ct = default);
}

public interface ITournoiRepository : IRepository<Tournoi>
{
    Task<PagedResult<Tournoi>> GetPagedAsync(PaginationParams pagination, int? jeuId = null, CancellationToken ct = default);
}

public interface IInscriptionRepository : IRepository<Inscription>
{
    Task<PagedResult<Inscription>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default);
    Task<Inscription?> GetByTournoiEtEquipeAsync(int tournoiId, int equipeId, CancellationToken ct = default);
    Task<List<Inscription>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default);
}

public interface IMatchRepository : IRepository<Match>
{
    Task<PagedResult<Match>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default);
    Task<Match?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Match>> GetByTournoiTermineAsync(int tournoiId, CancellationToken ct = default);
    Task<List<Match>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default);
}
