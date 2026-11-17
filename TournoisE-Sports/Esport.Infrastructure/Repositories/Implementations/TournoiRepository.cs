using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class TournoiRepository : ITournoiRepository
{
    private readonly AppDbContext _db;
    public TournoiRepository(AppDbContext db) => _db = db;

    public async Task<List<Tournoi>> GetAllAsync(CancellationToken ct = default)
        => await _db.Tournois.AsNoTracking().Include(t => t.Jeu).OrderByDescending(t => t.DateDebut).ToListAsync(ct);

    public async Task<PagedResult<Tournoi>> GetPagedAsync(PaginationParams pagination, int? jeuId = null, CancellationToken ct = default)
    {
        var query = _db.Tournois.AsNoTracking().Include(t => t.Jeu).Include(t => t.Inscriptions).Include(t => t.Matchs).AsQueryable();
        if (jeuId.HasValue)
            query = query.Where(t => t.JeuId == jeuId.Value);
        query = query.OrderByDescending(t => t.DateDebut);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Tournoi> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    Task<PagedResult<Tournoi>> IRepository<Tournoi>.GetPagedAsync(PaginationParams pagination, CancellationToken ct)
        => GetPagedAsync(pagination, null, ct);

    public async Task<Tournoi?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Tournois.Include(t => t.Jeu).Include(t => t.Inscriptions).Include(t => t.Matchs).FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task AddAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Tournois.Add(tournoi);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Entry(tournoi).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Tournois.Remove(tournoi);
        return Task.CompletedTask;
    }
}
