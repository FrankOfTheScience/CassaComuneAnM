using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using CassaComuneAnM.MauiAppUi.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class TripDetailViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _tripCode;
    private Trip? _trip;
    private bool _isEditingTrip;
    private CurrencyCode? _selectedCurrency;
    private string _exchangeRateInput = string.Empty;

    public ObservableCollection<ParticipantSummaryViewModel> ParticipantSummaries { get; } = new();
    public IReadOnlyList<CurrencyCode> SupportedCurrencies { get; } = Enum.GetValues<CurrencyCode>();

    public Trip? Trip
    {
        get => _trip;
        private set => SetProperty(ref _trip, value);
    }

    public bool IsEditingTrip
    {
        get => _isEditingTrip;
        set => SetProperty(ref _isEditingTrip, value);
    }

    public CurrencyCode? SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (SetProperty(ref _selectedCurrency, value))
            {
                if (Trip is not null && value.HasValue)
                {
                    Trip.Currency = value.Value.ToString();
                }

                OnPropertyChanged(nameof(ExchangeRatePlaceholder));
            }
        }
    }

    public string ExchangeRateInput
    {
        get => _exchangeRateInput;
        set => SetProperty(ref _exchangeRateInput, value);
    }

    public string ExchangeRatePlaceholder =>
        SelectedCurrency switch
        {
            null => "Cambio contro EUR, es. USD 1,10 = 1 EUR vale 1,10 USD",
            CurrencyCode.EUR => "Cambio contro EUR, per EUR lascia 1",
            _ => $"Cambio contro EUR, es. {SelectedCurrency} 1,10 = 1 EUR vale 1,10 {SelectedCurrency}"
        };

    public ICommand ManageParticipantsCommand { get; }
    public ICommand ManageExpensesCommand { get; }
    public ICommand ManageDepositsCommand { get; }
    public ICommand DeleteTripCommand { get; }
    public ICommand EditTripCommand { get; }
    public ICommand SaveTripCommand { get; }
    public ICommand CancelTripEditCommand { get; }

    public TripDetailViewModel(ITripService tripService, IServiceProvider serviceProvider, string tripCode)
    {
        _tripService = tripService;
        _serviceProvider = serviceProvider;
        _tripCode = tripCode;
        Title = "Dettaglio viaggio";

        ManageParticipantsCommand = new Command(async () => await Navigation!.PushAsync(new ParticipantPage(_serviceProvider, _tripCode)));
        ManageExpensesCommand = new Command(async () => await Navigation!.PushAsync(new ExpensePage(_serviceProvider, _tripCode)));
        ManageDepositsCommand = new Command(async () => await Navigation!.PushAsync(new DepositPage(_serviceProvider, _tripCode)));
        DeleteTripCommand = new Command(async () => await DeleteTripAsync());
        EditTripCommand = new Command(StartTripEdit);
        SaveTripCommand = new Command(async () => await SaveTripAsync());
        CancelTripEditCommand = new Command(CancelTripEdit);
    }

    public async Task LoadTripAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Trip = await _tripService.GetTripByCodeAsync(_tripCode);
            ParticipantSummaries.Clear();

            if (Trip is not null)
            {
                Title = Trip.TripName;
                SyncEditableFieldsFromTrip();

                foreach (var participant in Trip.Participants.OrderBy(p => p.Name))
                {
                    var totalPaid = participant.Deposits.Sum(d => d.Amount);
                    ParticipantSummaries.Add(new ParticipantSummaryViewModel
                    {
                        Name = participant.Name,
                        PersonalBudget = participant.PersonalBudget,
                        TotalPaid = totalPaid,
                        RemainingBudget = participant.PersonalBudget - totalPaid
                    });
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteTripAsync()
    {
        if (Trip is null)
        {
            return;
        }

        var confirmed = await ShowConfirmAsync("Elimina viaggio", $"Vuoi eliminare il viaggio '{Trip.TripName}'?");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _tripService.DeleteTripAsync(_tripCode);
            await Navigation!.PopAsync();
        });
    }

    private void StartTripEdit()
    {
        if (Trip is null)
        {
            return;
        }

        SyncEditableFieldsFromTrip();
        IsEditingTrip = true;
    }

    private void CancelTripEdit()
    {
        SyncEditableFieldsFromTrip();
        IsEditingTrip = false;
    }

    private async Task SaveTripAsync()
    {
        if (Trip is null)
        {
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

        await RunBusyAsync(async () =>
        {
            Trip.Currency = SelectedCurrency.Value.ToString();
            Trip.ExchangeRate = exchangeRate;
            await _tripService.SaveOrUpdateTripAsync(Trip);
            IsEditingTrip = false;
            await LoadTripAsync();
        });
    }

    private void SyncEditableFieldsFromTrip()
    {
        if (Trip is null)
        {
            SelectedCurrency = null;
            ExchangeRateInput = string.Empty;
            return;
        }

        SelectedCurrency = Enum.TryParse<CurrencyCode>(Trip.Currency, ignoreCase: true, out var parsedCurrency)
            ? parsedCurrency
            : null;
        ExchangeRateInput = FormatDecimalInput(Trip.ExchangeRate, "0.####");
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
                return true;
            }

            errorMessage = "Per EUR lascia il campo vuoto oppure inserisci 1.";
            return false;
        }

        if (TryParseDecimalInput(ExchangeRateInput, out var parsedRate) && parsedRate > 0)
        {
            exchangeRate = parsedRate;
            return true;
        }

        errorMessage = "Inserisci il valore della valuta locale corrispondente a 1 EUR. Esempio: USD 1,10 significa che 1 EUR vale 1,10 USD.";
        return false;
    }
}
