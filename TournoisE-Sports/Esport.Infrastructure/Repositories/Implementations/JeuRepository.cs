using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class JeuRepository : IJeuRepository
{
    private readonly AppDbContext _db;
    public JeuRepository(AppDbContext db) => _db = db;

    public async Task<List<Jeu>> GetAllAsync(CancellationToken ct = default)
        => await _db.Jeux.AsNoTracking().OrderBy(j => j.Nom).ToListAsync(ct);

    public async Task<PagedResult<Jeu>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Jeux.AsNoTracking().OrderBy(j => j.Nom);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Jeu> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public async Task<Jeu?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Jeux.FindAsync(new object[] { id }, ct);

    public Task AddAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Jeux.Add(jeu);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Entry(jeu).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Jeux.Remove(jeu);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string nom, int? excludeId = null, CancellationToken ct = default)
        => await _db.Jeux.AnyAsync(j => j.Nom.ToLower() == nom.ToLower() && j.Id != excludeId, ct);
}
