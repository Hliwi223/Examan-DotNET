using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class EquipeRepository : IEquipeRepository
{
    private readonly AppDbContext _db;
    public EquipeRepository(AppDbContext db) => _db = db;

    public async Task<List<Equipe>> GetAllAsync(CancellationToken ct = default)
        => await _db.Equipes.AsNoTracking().Include(e => e.Joueurs).OrderBy(e => e.Nom).ToListAsync(ct);

    public async Task<PagedResult<Equipe>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Equipes.AsNoTracking().Include(e => e.Joueurs).OrderBy(e => e.Nom);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Equipe> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public async Task<Equipe?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Equipes.FindAsync(new object[] { id }, ct);

    public async Task<Equipe?> GetByIdWithJoueursAsync(int id, CancellationToken ct = default)
        => await _db.Equipes.Include(e => e.Joueurs).FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task AddAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Equipes.Add(equipe);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Entry(equipe).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Equipes.Remove(equipe);
        return Task.CompletedTask;
    }
}
