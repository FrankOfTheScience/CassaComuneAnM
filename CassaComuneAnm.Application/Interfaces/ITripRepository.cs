using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;

namespace CassaComuneAnm.Application.Interfaces;
public interface ITripRepository : IRepository<Trip>
{
    // Recupera un trip con participants, expenses (e le loro ExpenseParticipants + participant) e deposits
    Task<Trip?> GetByCodeWithDetailsAsync(string tripCode);
}