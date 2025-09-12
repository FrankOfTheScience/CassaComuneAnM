using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;

namespace CassaComuneAnm.Application.Services;
public class TripService : ITripService
{
    private readonly ITripRepository _tripRepo;
    private readonly IRepository<Expense> _expenseRepo;
    private readonly IRepository<Deposit> _depositRepo;

    public TripService(
        ITripRepository tripRepo,
        IRepository<Expense> expenseRepo,
        IRepository<Deposit> depositRepo)
    {
        _tripRepo = tripRepo;
        _expenseRepo = expenseRepo;
        _depositRepo = depositRepo;
    }

    public async Task<IEnumerable<Trip>> GetAllTripsAsync() =>
        await _tripRepo.GetAllAsync();

    public async Task<Trip?> GetTripByCodeAsync(string tripCode) =>
        (await _tripRepo.GetAllAsync()).FirstOrDefault(t => t.TripCode == tripCode);

    public async Task CreateTripAsync(Trip trip)
    {
        await _tripRepo.AddAsync(trip);
        await _tripRepo.SaveChangesAsync();
    }

    public async Task SaveOrUpdateTripAsync(Trip trip)
    {
        await _tripRepo.UpdateAsync(trip);
        await _tripRepo.SaveChangesAsync();
    }

    public async Task DeleteTripAsync(string tripCode)
    {
        var trip = (await _tripRepo.GetAllAsync()).FirstOrDefault(t => t.TripCode == tripCode);
        if (trip != null)
        {
            await _tripRepo.DeleteAsync(trip);
            await _tripRepo.SaveChangesAsync();
        }
    }

    public async Task AddParticipantAsync(string tripCode, Participant participant)
    {
        var trip = await GetTripByCodeAsync(tripCode);
        if (trip == null) return;
        trip.Participants.Add(participant);
        await SaveOrUpdateTripAsync(trip);
    }

    public async Task RemoveParticipantAsync(string tripCode, string participantName)
    {
        var trip = await GetTripByCodeAsync(tripCode);
        if (trip == null) return;
        var p = trip.Participants.FirstOrDefault(x => x.Name == participantName);
        if (p != null) trip.Participants.Remove(p);
        await SaveOrUpdateTripAsync(trip);
    }

    public async Task AddExpenseAsync(string tripCode, DateTime date, string description, decimal amount, bool tourLeaderFree, List<string> excludedNames)
    {
        // Recupero il trip con tutti i dettagli (participants, expenses, deposits)
        var trip = await _tripRepo.GetByCodeWithDetailsAsync(tripCode);
        if (trip == null) throw new InvalidOperationException($"Trip with code {tripCode} not found");

        // Lista nomi partecipanti
        var allParticipants = trip.Participants.Select(p => p.Name).ToList();

        // Se ci sono esclusi -> i beneficiari sono tutti meno gli esclusi
        var beneficiaries = allParticipants.Except(excludedNames).ToList();

        int totalPeople = allParticipants.Count;
        int payersCount = beneficiaries.Count;

        if (totalPeople == 0) throw new InvalidOperationException("No participants in the trip.");

        decimal costPerPerson = amount / totalPeople;

        // Se TL free e TL è tra i beneficiari, togliamo il coordinatore dal conteggio dei paganti
        if (tourLeaderFree && beneficiaries.Contains(trip.CoordinatorName))
            payersCount--;

        // description arricchita se TL free
        if (tourLeaderFree && beneficiaries.Contains(trip.CoordinatorName))
            description += $" (TL Free of {costPerPerson:F2})";

        decimal effectiveTotal = costPerPerson * (totalPeople - (tourLeaderFree ? 1 : 0));
        decimal costPerPayer = payersCount > 0 ? effectiveTotal / payersCount : 0m;

        // --- crea la spesa principale ---
        var expense = new Expense
        {
            Date = date,
            Description = description,
            Amount = effectiveTotal,
            TourLeaderFree = tourLeaderFree,
            TripId = trip.Id,
            ExpenseParticipants = new List<ExpenseParticipant>()
        };

        // associa i partecipanti beneficiari (oggetti Participant già tracciati perché inclusi nella query)
        foreach (var bName in beneficiaries)
        {
            var participant = trip.Participants.FirstOrDefault(p => p.Name == bName);
            if (participant != null)
            {
                expense.ExpenseParticipants.Add(new ExpenseParticipant
                {
                    Expense = expense,
                    Participant = participant,
                    ParticipantId = participant.Id
                });
            }
        }

        // aggiungo la spesa al trip (ma non salvo ancora)
        trip.Expenses.Add(expense);

        // --- crea i rimborsi per gli esclusi ---
        foreach (var exName in excludedNames)
        {
            var participant = trip.Participants.FirstOrDefault(p => p.Name == exName);
            if (participant == null) continue;

            var refundAmount = tourLeaderFree ? costPerPayer : costPerPerson;

            var refundExpense = new Expense
            {
                Date = date,
                Description = $"Rimborso {description} ({exName})",
                Amount = refundAmount,
                TourLeaderFree = false,
                TripId = trip.Id,
                ExpenseParticipants = new List<ExpenseParticipant>
            {
                new ExpenseParticipant
                {
                    Expense = null!, // verrà impostato da EF quando si persiste refundExpense
                    Participant = participant,
                    ParticipantId = participant.Id
                }
            }
            };

            // Per sicurezza colleghiamo l'oggetto di navigazione
            refundExpense.ExpenseParticipants.First().Expense = refundExpense;

            trip.Expenses.Add(refundExpense);
        }

        // --- Persisti le modifiche (tutto in un'unica transazione salverà trip + expenses + join) ---
        await _tripRepo.SaveChangesAsync();
    }

    public async Task<List<Expense>> GetExpensesAsync(string tripCode)
    {
        var trip = await GetTripByCodeAsync(tripCode);
        return trip?.Expenses ?? new List<Expense>();
    }

    public async Task AddDepositAsync(string tripCode, string payerName, DateTime date, decimal amount)
    {
        var trip = await GetTripByCodeAsync(tripCode);
        if (trip == null)
            throw new InvalidOperationException($"Trip {tripCode} non trovato");

        var participant = trip.Participants.FirstOrDefault(p => p.Name == payerName);
        if (participant == null)
            throw new InvalidOperationException($"Partecipante {payerName} non trovato");

        if (amount <= 0)
            throw new ArgumentException("L'importo deve essere maggiore di zero.", nameof(amount));

        // Calcolo del residuo budget
        decimal giaVersato = participant.Deposits.Sum(d => d.Amount);
        decimal residuo = participant.PersonalBudget - giaVersato;

        if (amount > residuo)
        {
            // Logica: aumento budget per tutti i partecipanti
            decimal delta = amount - residuo;
            foreach (var p in trip.Participants)
                p.PersonalBudget += delta;
        }

        // Aggiungi il deposito
        var deposit = new Deposit
        {
            Date = date,
            Amount = amount,
            ParticipantId = participant.Id,
            Participant = participant,
            PayerName = payerName,
            TripId = trip.Id,
            Trip = trip
        };

        participant.Deposits.Add(deposit);
        trip.Deposits.Add(deposit);

        await _depositRepo.AddAsync(deposit);
        await _depositRepo.SaveChangesAsync();
        await SaveOrUpdateTripAsync(trip);
    }

    public async Task<List<Deposit>> GetDepositsAsync(string tripCode)
    {
        var trip = await GetTripByCodeAsync(tripCode);
        return trip?.Deposits ?? new List<Deposit>();
    }

    public async Task<decimal> GetTotalBudgetAsync(string tripCode) =>
        (await GetTripByCodeAsync(tripCode))?.TotalBudget ?? 0m;

    public async Task<decimal> GetTotalPaidAsync(string tripCode) =>
        (await GetTripByCodeAsync(tripCode))?.TotalPaid ?? 0m;

    public async Task<decimal> GetTotalExpensesAsync(string tripCode) =>
        (await GetTripByCodeAsync(tripCode))?.TotalExpenses ?? 0m;

    public async Task<decimal> GetCashBalanceAsync(string tripCode) =>
        (await GetTripByCodeAsync(tripCode))?.CashBalance ?? 0m;
}
