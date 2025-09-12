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
        => await _dbSet.ToListAsync();

    public async Task<Trip?> GetByIdAsync(object id)
        => await _dbSet.FindAsync(id);

    public async Task AddAsync(Trip entity)
    {
        _dbSet.Add(entity);
    }

    public async Task UpdateAsync(Trip entity)
    {
        _dbSet.Update(entity);
    }

    public async Task DeleteAsync(Trip entity)
    {
        _dbSet.Remove(entity);
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
            .Include(t => t.Deposits)
            .Include(t => t.Expenses)
                .ThenInclude(e => e.ExpenseParticipants)
                    .ThenInclude(ep => ep.Participant)
            .FirstOrDefaultAsync(t => t.TripCode == tripCode);
    }
}
