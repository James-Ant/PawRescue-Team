using Microsoft.EntityFrameworkCore;
using PawRescue.Application.Interfaces;
using PawRescue.Domain.Common;
using PawRescue.Infrastructure.Data;

namespace PawRescue.Infrastructure.Repositories;

/// <summary>
/// INHERITANCE: every concrete repository extends this generic base and
/// only adds the queries specific to its entity (DRY).
/// </summary>
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    protected Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
