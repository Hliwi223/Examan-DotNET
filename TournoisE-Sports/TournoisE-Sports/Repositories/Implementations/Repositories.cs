using Microsoft.EntityFrameworkCore;
using TournoisE_Sports.Common;
using TournoisE_Sports.Data;
using TournoisE_Sports.Models;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Repositories.Implementations;

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

    public async Task AddAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Jeux.Add(jeu);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Entry(jeu).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Jeu jeu, CancellationToken ct = default)
    {
        _db.Jeux.Remove(jeu);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(string nom, int? excludeId, CancellationToken ct = default)
        => await _db.Jeux.AnyAsync(j => j.Nom.ToLower() == nom.ToLower() && j.Id != excludeId, ct);
}

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

    public async Task AddAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Equipes.Add(equipe);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Entry(equipe).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Equipe equipe, CancellationToken ct = default)
    {
        _db.Equipes.Remove(equipe);
        await _db.SaveChangesAsync(ct);
    }
}

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

    public async Task<Joueur?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Joueurs.Include(j => j.Equipe).FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task AddAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Joueurs.Add(joueur);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Entry(joueur).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Joueur joueur, CancellationToken ct = default)
    {
        _db.Joueurs.Remove(joueur);
        await _db.SaveChangesAsync(ct);
    }
}

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

    public async Task<Tournoi?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Tournois.Include(t => t.Jeu).Include(t => t.Inscriptions).Include(t => t.Matchs).FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Tournois.Add(tournoi);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Entry(tournoi).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Tournoi tournoi, CancellationToken ct = default)
    {
        _db.Tournois.Remove(tournoi);
        await _db.SaveChangesAsync(ct);
    }
}

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

    public async Task AddAsync(Inscription inscription, CancellationToken ct = default)
    {
        _db.Inscriptions.Add(inscription);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Inscription inscription, CancellationToken ct = default)
    {
        _db.Inscriptions.Remove(inscription);
        await _db.SaveChangesAsync(ct);
    }
}

public class MatchRepository : IMatchRepository
{
    private readonly AppDbContext _db;
    public MatchRepository(AppDbContext db) => _db = db;

    public async Task<List<Match>> GetByTournoiAsync(int tournoiId, CancellationToken ct = default)
        => await _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .Where(m => m.TournoiId == tournoiId).OrderBy(m => m.DateMatch).ToListAsync(ct);

    public async Task<PagedResult<Match>> GetPagedByTournoiAsync(int tournoiId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .Where(m => m.TournoiId == tournoiId).OrderBy(m => m.DateMatch);
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync(ct);
        return new PagedResult<Match> { Items = items, Page = Math.Max(pagination.Page, 1), PageSize = pagination.Take, TotalCount = total };
    }

    public async Task<Match?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Matchs.Include(m => m.EquipeA).Include(m => m.EquipeB).Include(m => m.Vainqueur).Include(m => m.Tournoi)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<List<Match>> GetByTournoiTermineAsync(int tournoiId, CancellationToken ct = default)
        => await _db.Matchs.AsNoTracking().Include(m => m.EquipeA).Include(m => m.EquipeB)
            .Where(m => m.TournoiId == tournoiId && m.Termine).ToListAsync(ct);

    public async Task AddAsync(Match match, CancellationToken ct = default)
    {
        _db.Matchs.Add(match);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Match match, CancellationToken ct = default)
    {
        _db.Entry(match).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Match match, CancellationToken ct = default)
    {
        _db.Matchs.Remove(match);
        await _db.SaveChangesAsync(ct);
    }
}
