using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _db;
    public MatchRepository(AppDbContext db) => _db = db;

    public async Task<List<Domain.Entities.Match>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default)
        => await _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .Where(m => m.TournoiId == tournoiId).OrderBy(m => m.DateMatch).ToListAsync(ct);

    public async Task<PagedResult<Domain.Entities.Match>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .Where(m => m.TournoiId == tournoiId).OrderBy(m => m.DateMatch);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Domain.Entities.Match> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public async Task<Domain.Entities.Match?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Matchs.Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<List<Domain.Entities.Match>> GetByTournoiTermineAsync(int tournoiId, CancellationToken ct = default)
        => await _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB)
            .Where(m => m.TournoiId == tournoiId && m.Termine).ToListAsync(ct);

    public Task AddAsync(Domain.Entities.Match match, CancellationToken ct = default)
    {
        _db.Matchs.Add(match);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Domain.Entities.Match match, CancellationToken ct = default)
    {
        _db.Entry(match).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Match match, CancellationToken ct = default)
    {
        _db.Matchs.Remove(match);
        return Task.CompletedTask;
    }

    // IRepository<Match> members:
    public Task<List<Domain.Entities.Match>> GetAllAsync(CancellationToken ct = default)
        => _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi).OrderBy(m => m.DateMatch).ToListAsync(ct);

    public Task<PagedResult<Domain.Entities.Match>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi).OrderBy(m => m.DateMatch);
        return GetPagedFromQuery(query, pagination, ct);
    }

    private static async Task<PagedResult<Domain.Entities.Match>> GetPagedFromQuery(IQueryable<Domain.Entities.Match> query, PaginationParams pagination, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Domain.Entities.Match> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }
}
