using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using CassaComuneAnM.MauiAppUi.Services;
using System.Collections.ObjectModel;
using System.Globalization;
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
    private string _country = string.Empty;
    private CurrencyCode? _selectedCurrency;
    private string _exchangeRateInput = string.Empty;
    private string _budgetPerPaxInput = string.Empty;
    private string _participantName = string.Empty;
    private string _participantBudgetInput = string.Empty;

    public ObservableCollection<InitialParticipantViewModel> Participants { get; } = new();
    public IReadOnlyList<CurrencyOption> CurrencyOptions { get; } = CurrencyCatalog.All;

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
        set
        {
            if (SetProperty(ref _tripDate, value))
            {
                OnPropertyChanged(nameof(TripDateDisplay));
            }
        }
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

    public CurrencyCode? SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (SetProperty(ref _selectedCurrency, value))
            {
                OnPropertyChanged(nameof(SelectedCurrencyLabel));
                OnPropertyChanged(nameof(ExchangeRatePlaceholder));
                OnPropertyChanged(nameof(ExchangeRateHelpText));
            }
        }
    }

    public string ExchangeRateInput
    {
        get => _exchangeRateInput;
        set => SetProperty(ref _exchangeRateInput, value);
    }

    public string BudgetPerPaxInput
    {
        get => _budgetPerPaxInput;
        set => SetProperty(ref _budgetPerPaxInput, value);
    }

    public string ParticipantName
    {
        get => _participantName;
        set => SetProperty(ref _participantName, value);
    }

    public string ParticipantBudgetInput
    {
        get => _participantBudgetInput;
        set => SetProperty(ref _participantBudgetInput, value);
    }

    public string TripDateDisplay => TripDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("it-IT"));

    public string SelectedCurrencyLabel =>
        SelectedCurrency.HasValue
            ? CurrencyCatalog.GetDisplayLabel(SelectedCurrency.Value)
            : "SELEZIONA VALUTA LOCALE DEL VIAGGIO";

    public string ExchangeRatePlaceholder =>
        SelectedCurrency switch
        {
            null => "Cambio contro EUR, es. USD 1,10 = 1 EUR vale 1,10 USD",
            CurrencyCode.EUR => "Cambio contro EUR, per EUR lascia vuoto oppure inserisci 1",
            _ => $"Cambio contro EUR, es. {SelectedCurrency} 1,10 = 1 EUR vale 1,10 {SelectedCurrency}"
        };

    public string ExchangeRateHelpText =>
        CurrencyDisplayService.BuildExchangeRateHelp(SelectedCurrency ?? CurrencyCode.USD);

    public ICommand SaveTripCommand { get; }
    public ICommand AddParticipantCommand { get; }
    public ICommand RemoveParticipantCommand { get; }
    public ICommand SelectCurrencyCommand { get; }
    public ICommand SelectTripDateCommand { get; }

    public CreateTripViewModel(ITripService tripService)
    {
        _tripService = tripService;
        Title = "Nuovo viaggio";
        SaveTripCommand = new Command(async () => await SaveTripAsync());
        AddParticipantCommand = new Command(AddParticipant);
        RemoveParticipantCommand = new Command<InitialParticipantViewModel>(RemoveParticipant);
        SelectCurrencyCommand = new Command(async () => await SelectCurrencyAsync());
        SelectTripDateCommand = new Command(async () => await SelectTripDateAsync());
    }

    private void AddParticipant()
    {
        if (string.IsNullOrWhiteSpace(ParticipantName))
        {
            return;
        }

        var baseBudget = TryParseDecimalInput(BudgetPerPaxInput, out var parsedBaseBudget)
            ? parsedBaseBudget
            : 0m;
        var participantBudget = TryParseDecimalInput(ParticipantBudgetInput, out var parsedParticipantBudget)
            ? parsedParticipantBudget
            : baseBudget;

        Participants.Add(new InitialParticipantViewModel
        {
            Name = ParticipantName.Trim(),
            PersonalBudget = participantBudget
        });

        ParticipantName = string.Empty;
        ParticipantBudgetInput = string.Empty;
    }

    private void RemoveParticipant(InitialParticipantViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        Participants.Remove(participant);
    }

    private async Task SelectCurrencyAsync()
    {
        var selected = await ShowSelectionAsync(
            "Valuta del viaggio",
            "Scegli il currency code ISO e il nome italiano della valuta locale del viaggio.",
            CurrencyOptions,
            option => option.Label,
            CurrencyOptions.FirstOrDefault(option => option.Code == SelectedCurrency));

        if (selected is not null)
        {
            SelectedCurrency = selected.Code;
        }
    }

    private async Task SelectTripDateAsync()
    {
        var selectedDate = await ShowDatePickerAsync("Data viaggio", "Seleziona la data del viaggio.", TripDate);
        if (selectedDate.HasValue)
        {
            TripDate = selectedDate.Value;
            OnPropertyChanged(nameof(TripDateDisplay));
        }
    }

    private async Task SaveTripAsync()
    {
        if (string.IsNullOrWhiteSpace(TripName) ||
            string.IsNullOrWhiteSpace(TripCode) ||
            string.IsNullOrWhiteSpace(CoordinatorName) ||
            string.IsNullOrWhiteSpace(CoordinatorCode) ||
            string.IsNullOrWhiteSpace(CashierName) ||
            string.IsNullOrWhiteSpace(Country))
        {
            await ShowAlertAsync("Dati mancanti", "Compila i campi obbligatori del viaggio.");
            return;
        }

        if (!SelectedCurrency.HasValue)
        {
            await ShowAlertAsync("Valuta mancante", "Seleziona una valuta valida per il viaggio.");
            return;
        }

        if (!TryGetExchangeRate(out var exchangeRate, out var exchangeRateError))
        {
            await ShowAlertAsync("Cambio non valido", exchangeRateError);
            return;
        }

        var budgetPerPax = TryParseDecimalInput(BudgetPerPaxInput, out var parsedBudgetPerPax)
            ? parsedBudgetPerPax
            : 0m;

        await RunBusyAsync(async () =>
        {
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
                Currency = SelectedCurrency.Value.ToString(),
                ExchangeRate = exchangeRate,
                BudgetPerPax = budgetPerPax
            };

            foreach (var participant in Participants.Where(p => !string.IsNullOrWhiteSpace(p.Name)))
            {
                trip.Participants.Add(new Participant
                {
                    Name = participant.Name.Trim(),
                    Balance = 0m,
                    PersonalBudget = participant.PersonalBudget > 0 ? participant.PersonalBudget : budgetPerPax,
                    TripId = 0,
                    Trip = trip
                });
            }

            await _tripService.CreateTripAsync(trip);
            await ShowAlertAsync("Viaggio creato", "Il viaggio è stato salvato.");
            await Navigation!.PopAsync();
        });
    }

    private bool TryGetExchangeRate(out decimal exchangeRate, out string errorMessage)
    {
        exchangeRate = 1m;
        errorMessage = string.Empty;

        if (SelectedCurrency == CurrencyCode.EUR)
        {
            if (string.IsNullOrWhiteSpace(ExchangeRateInput))
            {
                return true;
            }

            if (TryParseDecimalInput(ExchangeRateInput, out var eurRate) && eurRate > 0)
            {
                exchangeRate = eurRate;
                ExchangeRateInput = FormatLocalizedDecimalInput(exchangeRate, "0.00");
                return true;
            }

            errorMessage = "Per EUR lascia il campo vuoto oppure inserisci 1.";
            return false;
        }

        if (TryParseDecimalInput(ExchangeRateInput, out var parsedRate) && parsedRate > 0)
        {
            exchangeRate = parsedRate;
            ExchangeRateInput = FormatLocalizedDecimalInput(exchangeRate, "0.00");
            return true;
        }

        errorMessage = "Inserisci quanta valuta locale corrisponde a 1 EUR. Esempio: USD 1,10 significa che 1 EUR vale 1,10 USD.";
        return false;
    }
}
