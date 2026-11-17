using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class JoueurRepository : IJoueurRepository
{
    private readonly AppDbContext _db;
    public JoueurRepository(AppDbContext db) => _db = db;

    public async Task<List<Joueur>> GetAllAsync(CancellationToken ct = default)
        => await _db.Joueurs.AsNoTracking().Include(j => j.Equipe).OrderBy(j => j.Pseudo).ToListAsync(ct);

    public async Task<PagedResult<Joueur>> GetPagedAsync(PaginationParams pagination, int? equipeId = null, CancellationToken ct = default)
    {
        var query = _db.Joueurs.AsNoTracking().Include(j => j.Equipe).AsQueryable();
        if (equipeId.HasValue)
            query = query.Where(j => j.EquipeId == equipeId.Value);
        query = query.OrderBy(j => j.Pseudo);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Joueur> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    Task<PagedResult<Joueur>> IRepository<Joueur>.GetPagedAsync(PaginationParams pagination, CancellationToken ct)
        => GetPagedAsync(pagination, null, ct);

    public async Task<Joueur?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Joueurs.Include(j => j.Equipe).FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task AddAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Joueurs.Add(joueur);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Entry(joueur).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Joueurs.Remove(joueur);
        return Task.CompletedTask;
    }
}
