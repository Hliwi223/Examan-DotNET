using Esport.Application.Common;
using Esport.Domain;

namespace Esport.Application.Interfaces;

/// <summary>
/// Contrat g\u00e9n\u00e9rique d'un d\u00e9p\u00f4t (repository).
/// Note : aucun appel \u00e0 SaveChanges ici. La validation est centralis\u00e9e
/// dans l'UnitOfWork (une seule transaction pour plusieurs modifications).
/// </summary>
/// <typeparam name="T">Type d'entit\u00e9 (h\u00e9rite de BaseEntity).</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<T>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
