using TournoisE_Sports.Common;
using TournoisE_Sports.Models;

namespace TournoisE_Sports.Repositories.Interfaces;

public interface IJeuRepository
{
    Task<List<Jeu>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Jeu>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<Jeu?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Jeu jeu, CancellationToken ct = default);
    Task UpdateAsync(Jeu jeu, CancellationToken ct = default);
    Task DeleteAsync(Jeu jeu, CancellationToken ct = default);
    Task<bool> ExistsAsync(string nom, int? excludeId, CancellationToken ct = default);
}

public interface IEquipeRepository
{
    Task<List<Equipe>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Equipe>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<Equipe?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Equipe?> GetByIdWithJoueursAsync(int id, CancellationToken ct = default);
    Task AddAsync(Equipe equipe, CancellationToken ct = default);
    Task UpdateAsync(Equipe equipe, CancellationToken ct = default);
    Task DeleteAsync(Equipe equipe, CancellationToken ct = default);
}

public interface IJoueurRepository
{
    Task<List<Joueur>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Joueur>> GetPagedAsync(PaginationParams pagination, int? equipeId = null, CancellationToken ct = default);
    Task<Joueur?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Joueur joueur, CancellationToken ct = default);
    Task UpdateAsync(Joueur joueur, CancellationToken ct = default);
    Task DeleteAsync(Joueur joueur, CancellationToken ct = default);
}

public interface ITournoiRepository
{
    Task<List<Tournoi>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Tournoi>> GetPagedAsync(PaginationParams pagination, int? jeuId = null, CancellationToken ct = default);
    Task<Tournoi?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Tournoi tournoi, CancellationToken ct = default);
    Task UpdateAsync(Tournoi tournoi, CancellationToken ct = default);
    Task DeleteAsync(Tournoi tournoi, CancellationToken ct = default);
}

public interface IInscriptionRepository
{
    Task<List<Inscription>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default);
    Task<PagedResult<Inscription>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default);
    Task<Inscription?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Inscription?> GetByTournoiEtEquipeAsync(int tournoiId, int equipeId, CancellationToken ct = default);
    Task AddAsync(Inscription inscription, CancellationToken ct = default);
    Task DeleteAsync(Inscription inscription, CancellationToken ct = default);
}

public interface IMatchRepository
{
    Task<List<Match>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default);
    Task<PagedResult<Match>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default);
    Task<Match?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Match>> GetByTournoiTermineAsync(int tournoiId, CancellationToken ct = default);
    Task AddAsync(Match match, CancellationToken ct = default);
    Task UpdateAsync(Match match, CancellationToken ct = default);
    Task DeleteAsync(Match match, CancellationToken ct = default);
}
