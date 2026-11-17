using Microsoft.EntityFrameworkCore;
using Esport.Application.Common;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Infrastructure.Data;

namespace Esport.Infrastructure.Repositories.Implementations;

public class InscriptionRepository : IInscriptionRepository
{
    private readonly AppDbContext _db;
    public InscriptionRepository(AppDbContext db) => _db = db;

    public async Task<List<Inscription>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default)
        => await _db.Inscriptions.AsNoTracking().Include(i => i.Equipe).Include(i => i.Tournoi)
            .Where(i => i.TournoiId == tournoiId).OrderBy(i => i.DateInscription).ToListAsync(ct);

    public async Task<PagedResult<Inscription>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Inscriptions.AsNoTracking().Include(i => i.Equipe).Include(i => i.Tournoi)
            .Where(i => i.TournoiId == tournoiId).OrderBy(i => i.DateInscription);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Inscription> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public async Task<Inscription?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Inscriptions.Include(i => i.Equipe).Include(i => i.Tournoi).FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Inscription?> GetByTournoiEtEquipeAsync(int tournoiId, int equipeId, CancellationToken ct = default)
        => await _db.Inscriptions.FirstOrDefaultAsync(i => i.TournoiId == tournoiId && i.EquipeId == equipeId, ct);

    public Task AddAsync(Inscription inscription, CancellationToken ct = default)
    {
        _db.Inscriptions.Add(inscription);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Inscription inscription, CancellationToken ct = default)
    {
        _db.Inscriptions.Remove(inscription);
        return Task.CompletedTask;
    }

    // IRepository<Inscription> members not specific to inscriptions:
    public Task<List<Inscription>> GetAllAsync(CancellationToken ct = default)
        => _db.Inscriptions.AsNoTracking().Include(i => i.Equipe).Include(i => i.Tournoi).OrderBy(i => i.DateInscription).ToListAsync(ct);

    public Task<PagedResult<Inscription>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Inscriptions.AsNoTracking().Include(i => i.Equipe).Include(i => i.Tournoi).OrderBy(i => i.DateInscription);
        return GetPagedFromQuery(query, pagination, ct);
    }

    private static async Task<PagedResult<Inscription>> GetPagedFromQuery(IQueryable<Inscription> query, PaginationParams pagination, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Inscription> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public Task UpdateAsync(Inscription inscription, CancellationToken ct = default)
    {
        _db.Entry(inscription).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}
