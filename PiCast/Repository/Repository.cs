using Microsoft.EntityFrameworkCore;
using PiCast.Model;

namespace PiCast.Repository;

public interface IReadOnlyRepository<T> where T : Entity
{
    IQueryable<T> Get();
    Task<T> Get(int id);
}
public interface IRepository<T> : IReadOnlyRepository<T> where T : Entity
{
    Task<T> Add(T entity);
    Task<T> Update(int id, T entity);
    Task<int> Delete(int id);
}

public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : Entity
{
    protected readonly EntityContext _context;
    public virtual DbSet<T> Items => _context.Set<T>();

    public ReadOnlyRepository(EntityContext context)
    {
        _context = context;
    }

    public IQueryable<T> Get()
        => Items;

    public Task<T> Get(int id)
        => Items.SingleOrDefaultAsync(x => x.Id == id);
}

public class Repository<T> : ReadOnlyRepository<T>, IRepository<T> where T : Entity
{
    public Repository(EntityContext context) : base(context)
    { }

    private async Task<int> Save()
        => await _context.SaveChangesAsync();

    public async Task<T> Add(T entity)
    {
        var dbEntity = await Items.AddAsync(entity);
        await Save();
        return dbEntity.Entity;
    }

    public async Task<T> Update(int id, T entity)
    {
        entity.Id = id;
        var dbEntity = Items.Update(entity);
        await Save();
        return dbEntity.Entity;
    }

    public async Task<int> Delete(int id)
    {
        var dbEntity = await Items.SingleOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
            return 0;

        Items.Remove(dbEntity);
        return await Save();
    }
}