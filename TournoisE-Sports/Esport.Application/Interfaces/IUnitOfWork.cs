using Esport.Application.Common;

namespace Esport.Application.Interfaces;

/// <summary>
/// Unit of Work : coordonne plusieurs d\u00e9p\u00f4ts et valide toutes les
/// modifications en une seule transaction via SaveChangesAsync().
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IJeuRepository Jeux { get; }
    IEquipeRepository Equipes { get; }
    IJoueurRepository Joueurs { get; }
    ITournoiRepository Tournois { get; }
    IInscriptionRepository Inscriptions { get; }
    IMatchRepository Matchs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
