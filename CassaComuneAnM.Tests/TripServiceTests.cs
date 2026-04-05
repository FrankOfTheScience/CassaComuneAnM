using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnm.Application.Services;
using CassaComuneAnM.Core.Entities;

namespace CassaComuneAnM.Tests;

public class TripServiceTests
{
    [Fact]
    public async Task AddExpenseAsync_WithoutParticipants_ThrowsInvalidOperationException()
    {
        var trip = CreateTrip();
        var sut = CreateSut(trip);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddExpenseAsync(trip.TripCode, new DateTime(2026, 4, 5), "Museo", 100m, false, []));
    }

    [Fact]
    public async Task AddExpenseAsync_WithoutDeposits_CreatesExpense_AndCashBalanceBecomesNegative()
    {
        var trip = CreateTripWithParticipants(("Mario", 200m), ("Luigi", 200m));
        var sut = CreateSut(trip);

        await sut.AddExpenseAsync(trip.TripCode, new DateTime(2026, 4, 5), "Taxi", 50m, false, []);

        Assert.Single(trip.Expenses);
        Assert.Equal(50m, trip.TotalExpenses);
        Assert.Equal(0m, trip.TotalPaid);
        Assert.Equal(-50m, trip.CashBalance);
    }

    [Fact]
    public async Task AddExpenseAsync_WithExcludedParticipant_CreatesMainExpenseAndRefundExpense()
    {
        var trip = CreateTripWithParticipants(("Mario", 200m), ("Luigi", 200m), ("Anna", 200m));
        var sut = CreateSut(trip);

        await sut.AddExpenseAsync(trip.TripCode, new DateTime(2026, 4, 5), "Escursione", 90m, false, ["Anna"]);

        Assert.Equal(2, trip.Expenses.Count);

        var mainExpense = trip.Expenses.Single(expense => expense.Description == "Escursione");
        var refundExpense = trip.Expenses.Single(expense => expense.Description == "Rimborso Escursione (Anna)");

        Assert.Equal(90m, mainExpense.Amount);
        Assert.Equal(30m, refundExpense.Amount);
        Assert.Equal(["Luigi", "Mario"], mainExpense.ExpenseParticipants.Select(p => p.Participant.Name).OrderBy(name => name).ToArray());
        Assert.Equal(["Anna"], refundExpense.ExpenseParticipants.Select(p => p.Participant.Name).ToArray());
        Assert.Equal(120m, trip.TotalExpenses);
    }

    [Fact]
    public async Task AddDepositAsync_AboveRemainingBudget_WithoutIncrease_ThrowsInvalidOperationException()
    {
        var trip = CreateTripWithParticipants(("Mario", 100m), ("Luigi", 100m));
        var sut = CreateSut(trip);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddDepositAsync(trip.TripCode, "Mario", new DateTime(2026, 4, 5), 120m, allowBudgetIncrease: false));

        Assert.Empty(trip.Deposits);
        Assert.All(trip.Participants, participant => Assert.Equal(100m, participant.PersonalBudget));
    }

    [Fact]
    public async Task AddDepositAsync_AboveRemainingBudget_WithIncrease_ExpandsBudgetForAllParticipants()
    {
        var trip = CreateTripWithParticipants(("Mario", 100m), ("Luigi", 100m));
        var sut = CreateSut(trip);

        await sut.AddDepositAsync(trip.TripCode, "Mario", new DateTime(2026, 4, 5), 120m, allowBudgetIncrease: true);

        Assert.Single(trip.Deposits);
        Assert.All(trip.Participants, participant => Assert.Equal(120m, participant.PersonalBudget));
        Assert.Equal(240m, trip.TotalBudget);
        Assert.Equal(120m, trip.TotalPaid);
    }

    [Fact]
    public async Task UpdateDepositAsync_ReassignsDepositToAnotherParticipant_AndUpdatesCollections()
    {
        var trip = CreateTripWithParticipants(("Mario", 200m), ("Luigi", 200m));
        var existingDeposit = new Deposit
        {
            Id = 10,
            Date = new DateTime(2026, 4, 5),
            Amount = 80m,
            PayerName = "Mario",
            ParticipantId = trip.Participants[0].Id,
            Participant = trip.Participants[0],
            TripId = trip.Id,
            Trip = trip
        };
        trip.Participants[0].Deposits.Add(existingDeposit);
        trip.Deposits.Add(existingDeposit);

        var sut = CreateSut(trip);

        await sut.UpdateDepositAsync(trip.TripCode, 10, "Luigi", new DateTime(2026, 4, 6), 90m);

        Assert.Empty(trip.Participants[0].Deposits);
        Assert.Single(trip.Participants[1].Deposits);
        Assert.Equal("Luigi", existingDeposit.PayerName);
        Assert.Equal(trip.Participants[1].Id, existingDeposit.ParticipantId);
        Assert.Equal(90m, existingDeposit.Amount);
        Assert.Equal(new DateTime(2026, 4, 6), existingDeposit.Date);
    }

    private static TripService CreateSut(Trip trip)
    {
        var tripRepository = new InMemoryTripRepository([trip]);
        var expenseRepository = new InMemoryRepository<Expense>(trip.Expenses);
        var depositRepository = new InMemoryRepository<Deposit>(trip.Deposits);
        return new TripService(tripRepository, expenseRepository, depositRepository);
    }

    private static Trip CreateTrip()
    {
        return new Trip
        {
            Id = 1,
            TripName = "Test Trip",
            TripCode = "TEST01",
            TripDate = new DateTime(2026, 4, 5),
            CoordinatorName = "Mario",
            CoordinatorCode = "M01",
            CashierName = "Luigi",
            Country = "Italia",
            Currency = "EUR",
            ExchangeRate = 1m,
            BudgetPerPax = 0m
        };
    }

    private static Trip CreateTripWithParticipants(params (string Name, decimal Budget)[] participants)
    {
        var trip = CreateTrip();
        var nextId = 1;

        foreach (var participantData in participants)
        {
            var participant = new Participant
            {
                Id = nextId++,
                Name = participantData.Name,
                Balance = 0m,
                PersonalBudget = participantData.Budget,
                TripId = trip.Id,
                Trip = trip
            };

            trip.Participants.Add(participant);
        }

        trip.BudgetPerPax = participants.FirstOrDefault().Budget;
        return trip;
    }

    private sealed class InMemoryTripRepository : ITripRepository
    {
        private readonly List<Trip> _trips;

        public InMemoryTripRepository(IEnumerable<Trip> trips)
        {
            _trips = trips.ToList();
        }

        public Task<List<Trip>> GetAllAsync() => Task.FromResult(_trips);

        public Task<Trip?> GetByIdAsync(object id) =>
            Task.FromResult(_trips.FirstOrDefault(trip => trip.Id == (int)id));

        public Task<Trip?> GetByCodeWithDetailsAsync(string tripCode) =>
            Task.FromResult(_trips.FirstOrDefault(trip => trip.TripCode == tripCode));

        public Task AddAsync(Trip entity)
        {
            _trips.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Trip entity) => Task.CompletedTask;

        public Task DeleteAsync(Trip entity)
        {
            _trips.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private sealed class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _items;

        public InMemoryRepository(List<T> items)
        {
            _items = items;
        }

        public Task<List<T>> GetAllAsync() => Task.FromResult(_items.ToList());

        public Task<T?> GetByIdAsync(object id) => Task.FromResult<T?>(default);

        public Task AddAsync(T entity)
        {
            if (!_items.Contains(entity))
            {
                _items.Add(entity);
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity) => Task.CompletedTask;

        public Task DeleteAsync(T entity)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => Task.CompletedTask;
    }
}
