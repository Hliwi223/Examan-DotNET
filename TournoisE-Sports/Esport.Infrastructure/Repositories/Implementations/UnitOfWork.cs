using Esport.Application.Interfaces;
using Esport.Infrastructure.Data;
using Esport.Infrastructure.Repositories.Implementations;

namespace Esport.Infrastructure.Repositories.Implementations;

/// <summary>
/// Unit of Work : garde une r\u00e9f\u00e9rence \u00e0 chaque d\u00e9p\u00f4t et valide toutes les
/// modifications en une seule transaction via une seule appel \u00e0 SaveChangesAsync().
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public IJeuRepository Jeux { get; }
    public IEquipeRepository Equipes { get; }
    public IJoueurRepository Joueurs { get; }
    public ITournoiRepository Tournois { get; }
    public IInscriptionRepository Inscriptions { get; }
    public IMatchRepository Matchs { get; }

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Jeux = new JeuRepository(db);
        Equipes = new EquipeRepository(db);
        Joueurs = new JoueurRepository(db);
        Tournois = new TournoiRepository(db);
        Inscriptions = new InscriptionRepository(db);
        Matchs = new MatchRepository(db);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}
