using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CassaComuneAnM.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public Task<List<T>> GetAllAsync() => _dbSet.ToListAsync();

    public Task<T?> GetByIdAsync(object id) => _dbSet.FindAsync(id).AsTask();

    public Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
