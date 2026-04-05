using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CassaComuneAnM.Infrastructure.Repositories;
public class EfTripRepository : ITripRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Trip> _dbSet;

    public EfTripRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Trip>();
    }

    public async Task<List<Trip>> GetAllAsync()
        => await _dbSet
            .Include(t => t.Participants)
            .Include(t => t.Deposits)
            .Include(t => t.Expenses)
            .ToListAsync();

    public async Task<Trip?> GetByIdAsync(object id)
        => await _dbSet
            .Include(t => t.Participants)
                .ThenInclude(p => p.Deposits)
            .Include(t => t.Deposits)
            .Include(t => t.Expenses)
                .ThenInclude(e => e.ExpenseParticipants)
                    .ThenInclude(ep => ep.Participant)
            .FirstOrDefaultAsync(t => t.Id == (int)id);

    public Task AddAsync(Trip entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Trip entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Trip entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // IMPLEMENTAZIONE SPECIFICA
    public async Task<Trip?> GetByCodeWithDetailsAsync(string tripCode)
    {
        return await _context.Trips
            .Include(t => t.Participants)
                .ThenInclude(p => p.Deposits)
            .Include(t => t.Deposits)
            .Include(t => t.Expenses)
                .ThenInclude(e => e.ExpenseParticipants)
                    .ThenInclude(ep => ep.Participant)
            .FirstOrDefaultAsync(t => t.TripCode == tripCode);
    }
}
