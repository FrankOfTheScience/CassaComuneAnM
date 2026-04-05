using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class CreateTripViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private string _tripName = string.Empty;
    private string _tripCode = string.Empty;
    private DateTime _tripDate = DateTime.Today;
    private string _coordinatorName = string.Empty;
    private string _coordinatorCode = string.Empty;
    private string _cashierName = string.Empty;
    private string _country = "Italia";
    private string _currency = "EUR";
    private decimal _exchangeRate = 1m;
    private decimal _budgetPerPax;
    private string _participantName = string.Empty;
    private decimal _participantBudget;

    public ObservableCollection<InitialParticipantViewModel> Participants { get; } = new();

    public string TripName
    {
        get => _tripName;
        set => SetProperty(ref _tripName, value);
    }

    public string TripCode
    {
        get => _tripCode;
        set => SetProperty(ref _tripCode, value);
    }

    public DateTime TripDate
    {
        get => _tripDate;
        set => SetProperty(ref _tripDate, value);
    }

    public string CoordinatorName
    {
        get => _coordinatorName;
        set => SetProperty(ref _coordinatorName, value);
    }

    public string CoordinatorCode
    {
        get => _coordinatorCode;
        set => SetProperty(ref _coordinatorCode, value);
    }

    public string CashierName
    {
        get => _cashierName;
        set => SetProperty(ref _cashierName, value);
    }

    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    public string Currency
    {
        get => _currency;
        set => SetProperty(ref _currency, value);
    }

    public decimal ExchangeRate
    {
        get => _exchangeRate;
        set => SetProperty(ref _exchangeRate, value);
    }

    public decimal BudgetPerPax
    {
        get => _budgetPerPax;
        set => SetProperty(ref _budgetPerPax, value);
    }

    public string ParticipantName
    {
        get => _participantName;
        set => SetProperty(ref _participantName, value);
    }

    public decimal ParticipantBudget
    {
        get => _participantBudget;
        set => SetProperty(ref _participantBudget, value);
    }

    public ICommand SaveTripCommand { get; }
    public ICommand AddParticipantCommand { get; }
    public ICommand RemoveParticipantCommand { get; }

    public CreateTripViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Nuovo viaggio";
        SaveTripCommand = new Command(async () => await SaveTripAsync());
        AddParticipantCommand = new Command(AddParticipant);
        RemoveParticipantCommand = new Command<InitialParticipantViewModel>(RemoveParticipant);
    }

    private void AddParticipant()
    {
        if (string.IsNullOrWhiteSpace(ParticipantName))
        {
            return;
        }

        Participants.Add(new InitialParticipantViewModel
        {
            Name = ParticipantName.Trim(),
            PersonalBudget = ParticipantBudget > 0 ? ParticipantBudget : BudgetPerPax
        });

        ParticipantName = string.Empty;
        ParticipantBudget = 0m;
    }

    private void RemoveParticipant(InitialParticipantViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        Participants.Remove(participant);
    }

    private async Task SaveTripAsync()
    {
        if (string.IsNullOrWhiteSpace(TripName) ||
            string.IsNullOrWhiteSpace(TripCode) ||
            string.IsNullOrWhiteSpace(CoordinatorName) ||
            string.IsNullOrWhiteSpace(CoordinatorCode) ||
            string.IsNullOrWhiteSpace(CashierName))
        {
            await ShowAlertAsync("Dati mancanti", "Compila i campi obbligatori del viaggio.");
            return;
        }

        var normalizedTripCode = TripCode.Trim().ToUpperInvariant();
        var existingTrip = await _tripService.GetTripByCodeAsync(normalizedTripCode);
        if (existingTrip is not null)
        {
            await ShowAlertAsync("Codice duplicato", "Esiste già un viaggio con questo codice.");
            return;
        }

        var trip = new Trip
        {
            TripName = TripName.Trim(),
            TripCode = normalizedTripCode,
            TripDate = TripDate,
            CoordinatorName = CoordinatorName.Trim(),
            CoordinatorCode = CoordinatorCode.Trim().ToUpperInvariant(),
            CashierName = CashierName.Trim(),
            Country = Country.Trim(),
            Currency = Currency.Trim().ToUpperInvariant(),
            ExchangeRate = ExchangeRate <= 0 ? 1m : ExchangeRate,
            BudgetPerPax = BudgetPerPax
        };

        foreach (var participant in Participants.Where(p => !string.IsNullOrWhiteSpace(p.Name)))
        {
            trip.Participants.Add(new Participant
            {
                Name = participant.Name.Trim(),
                Balance = 0m,
                PersonalBudget = participant.PersonalBudget > 0 ? participant.PersonalBudget : BudgetPerPax,
                TripId = 0,
                Trip = trip
            });
        }

        await _tripService.CreateTripAsync(trip);
        await ShowAlertAsync("Viaggio creato", "Il viaggio è stato salvato.");
        await Navigation!.PopAsync();
    }
}
