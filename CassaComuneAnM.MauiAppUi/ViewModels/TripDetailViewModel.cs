using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using CassaComuneAnM.MauiAppUi.Services;
using CassaComuneAnM.MauiAppUi.Views;
using Microsoft.Maui.Graphics;
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
    public IReadOnlyList<CurrencyOption> CurrencyOptions { get; } = CurrencyCatalog.All;

    public Trip? Trip
    {
        get => _trip;
        private set
        {
            if (SetProperty(ref _trip, value))
            {
                OnTripChanged();
            }
        }
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

    public string SelectedCurrencyLabel =>
        SelectedCurrency.HasValue
            ? CurrencyCatalog.GetDisplayLabel(SelectedCurrency.Value)
            : "SELEZIONA VALUTA LOCALE DEL VIAGGIO";

    public string ExchangeRatePlaceholder =>
        SelectedCurrency switch
        {
            null => "Cambio contro EUR, es. USD 1,10 = 1 EUR vale 1,10 USD",
            CurrencyCode.EUR => "Cambio contro EUR, per EUR lascia 1",
            _ => $"Cambio contro EUR, es. {SelectedCurrency} 1,10 = 1 EUR vale 1,10 {SelectedCurrency}"
        };

    public string ExchangeRateHelpText =>
        CurrencyDisplayService.BuildExchangeRateHelp(SelectedCurrency ?? CurrencyCode.USD);

    public string CurrencySummary => Trip is null
        ? string.Empty
        : CurrencyCatalog.GetDisplayLabel(CurrencyDisplayService.GetTripCurrency(Trip));

    public string TotalBudgetPrimaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatPrimaryAmount(Trip.TotalBudget, Trip);

    public string TotalBudgetSecondaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatSecondaryEurAmount(Trip.TotalBudget, Trip);

    public string TotalPaidPrimaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatPrimaryAmount(Trip.TotalPaid, Trip);

    public string TotalPaidSecondaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatSecondaryEurAmount(Trip.TotalPaid, Trip);

    public string TotalExpensesPrimaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatPrimaryAmount(Trip.TotalExpenses, Trip);

    public string TotalExpensesSecondaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatSecondaryEurAmount(Trip.TotalExpenses, Trip);

    public string CashBalancePrimaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatPrimaryAmount(Trip.CashBalance, Trip);

    public string CashBalanceSecondaryDisplay => Trip is null
        ? "—"
        : CurrencyDisplayService.FormatSecondaryEurAmount(Trip.CashBalance, Trip);

    public bool IsCashBalanceNegative => Trip?.CashBalance < 0m;

    public Color CashBalanceColor => IsCashBalanceNegative
        ? Color.FromArgb("#B3261E")
        : Color.FromArgb("#242626");

    public string CashDeficitWarningText => !IsCashBalanceNegative || Trip is null
        ? string.Empty
        : $"Disavanzo attuale: {CurrencyDisplayService.FormatAmountWithEur(Math.Abs(Trip.CashBalance), Trip)}. Registra un versamento o copri il saldo il prima possibile.";

    public ICommand ManageParticipantsCommand { get; }
    public ICommand ManageExpensesCommand { get; }
    public ICommand ManageDepositsCommand { get; }
    public ICommand DeleteTripCommand { get; }
    public ICommand EditTripCommand { get; }
    public ICommand SaveTripCommand { get; }
    public ICommand CancelTripEditCommand { get; }
    public ICommand SelectCurrencyCommand { get; }

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
        SelectCurrencyCommand = new Command(async () => await SelectCurrencyAsync());
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
                    var remainingBudget = participant.PersonalBudget - totalPaid;
                    ParticipantSummaries.Add(new ParticipantSummaryViewModel
                    {
                        Name = participant.Name,
                        PersonalBudgetInEur = participant.PersonalBudget,
                        PersonalBudgetPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(participant.PersonalBudget, Trip),
                        PersonalBudgetSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(participant.PersonalBudget, Trip),
                        TotalPaidInEur = totalPaid,
                        TotalPaidPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(totalPaid, Trip),
                        TotalPaidSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(totalPaid, Trip),
                        RemainingBudgetInEur = remainingBudget,
                        RemainingBudgetPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(remainingBudget, Trip),
                        RemainingBudgetSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(remainingBudget, Trip)
                    });
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
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

        SelectedCurrency = Enum.TryParse<CurrencyCode>(Trip.Currency, true, out var parsedCurrency)
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

        errorMessage = "Inserisci quanta valuta locale corrisponde a 1 EUR. Esempio: USD 1,10 significa che 1 EUR vale 1,10 USD.";
        return false;
    }

    private void OnTripChanged()
    {
        OnPropertyChanged(nameof(CurrencySummary));
        OnPropertyChanged(nameof(TotalBudgetPrimaryDisplay));
        OnPropertyChanged(nameof(TotalBudgetSecondaryDisplay));
        OnPropertyChanged(nameof(TotalPaidPrimaryDisplay));
        OnPropertyChanged(nameof(TotalPaidSecondaryDisplay));
        OnPropertyChanged(nameof(TotalExpensesPrimaryDisplay));
        OnPropertyChanged(nameof(TotalExpensesSecondaryDisplay));
        OnPropertyChanged(nameof(CashBalancePrimaryDisplay));
        OnPropertyChanged(nameof(CashBalanceSecondaryDisplay));
        OnPropertyChanged(nameof(IsCashBalanceNegative));
        OnPropertyChanged(nameof(CashBalanceColor));
        OnPropertyChanged(nameof(CashDeficitWarningText));
    }
}
