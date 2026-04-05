using CassaComuneAnM.Core.Entities;

namespace CassaComuneAnm.Application.Interfaces;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetAllTripsAsync();
    Task CreateTripAsync(Trip trip);
    Task SaveOrUpdateTripAsync(Trip trip);

    Task<Trip?> GetTripByCodeAsync(string tripCode);
    Task DeleteTripAsync(string tripCode);

    // Gestione partecipanti
    Task AddParticipantAsync(string tripCode, Participant participant);
    Task RemoveParticipantAsync(string tripCode, string participantName);

    // Gestione spese
    Task AddExpenseAsync(string tripCode, DateTime date, string description, decimal amount, bool tourLeaderFree, List<string> excludedNames);
    Task<List<Expense>> GetExpensesAsync(string tripCode);
    Task DeleteExpenseAsync(string tripCode, int expenseId);
    Task UpdateExpenseAsync(string tripCode, int expenseId, DateTime date, string description, decimal amount, bool tourLeaderFree, List<string> excludedNames);

    // Gestione versamenti
    Task AddDepositAsync(string tripCode, string payerName, DateTime date, decimal amount, bool allowBudgetIncrease = false);
    Task<List<Deposit>> GetDepositsAsync(string tripCode);
    Task DeleteDepositAsync(string tripCode, int depositId);
    Task UpdateDepositAsync(string tripCode, int depositId, string payerName, DateTime date, decimal amount, bool allowBudgetIncrease = false);

    // Eventuali calcoli aggregati
    Task<decimal> GetTotalBudgetAsync(string tripCode);
    Task<decimal> GetTotalPaidAsync(string tripCode);
    Task<decimal> GetTotalExpensesAsync(string tripCode);
    Task<decimal> GetCashBalanceAsync(string tripCode);
}
