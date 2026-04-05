using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using CassaComuneAnM.MauiAppUi.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class DepositViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private Trip? _trip;
    private Participant? _selectedParticipant;
    private CurrencyCode _selectedInputCurrency = CurrencyCode.EUR;
    private string _amountInput = string.Empty;
    private DateTime _depositDate = DateTime.Today;
    private int? _editingDepositId;
    private string _submitButtonText = "Registra versamento";
    private bool _isEditing;
    private string _budgetPreviewText = "Seleziona un partecipante per vedere il residuo disponibile.";

    public ObservableCollection<Participant> Participants { get; } = new();
    public ObservableCollection<DepositHistoryItemViewModel> Deposits { get; } = new();

    public Participant? SelectedParticipant
    {
        get => _selectedParticipant;
        set
        {
            if (SetProperty(ref _selectedParticipant, value))
            {
                OnPropertyChanged(nameof(SelectedParticipantLabel));
                UpdateBudgetPreview();
            }
        }
    }

    public CurrencyCode SelectedInputCurrency
    {
        get => _selectedInputCurrency;
        set
        {
            if (_selectedInputCurrency == value)
            {
                return;
            }

            var previous = _selectedInputCurrency;
            _selectedInputCurrency = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedInputCurrencyLabel));
            OnPropertyChanged(nameof(AmountPlaceholder));
            ConvertAmountInputBetweenModes(previous, value);
            UpdateBudgetPreview();
        }
    }

    public string AmountInput
    {
        get => _amountInput;
        set
        {
            if (SetProperty(ref _amountInput, value))
            {
                UpdateBudgetPreview();
            }
        }
    }

    public DateTime DepositDate
    {
        get => _depositDate;
        set
        {
            if (SetProperty(ref _depositDate, value))
            {
                OnPropertyChanged(nameof(DepositDateDisplay));
            }
        }
    }

    public string BudgetPreviewText
    {
        get => _budgetPreviewText;
        set => SetProperty(ref _budgetPreviewText, value);
    }

    public string SubmitButtonText
    {
        get => _submitButtonText;
        set => SetProperty(ref _submitButtonText, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public string SelectedParticipantLabel => SelectedParticipant?.Name?.ToUpperInvariant() ?? "SELEZIONA IL PARTECIPANTE CHE VERSA";

    public string SelectedInputCurrencyLabel =>
        _trip is null
            ? "INSERIMENTO IN EUR"
            : CurrencyDisplayService.FormatInputModeLabel(SelectedInputCurrency, _trip);

    public string AmountPlaceholder =>
        SelectedInputCurrency == CurrencyCode.EUR
            ? "Importo versamento in EUR"
            : $"Importo versamento in {SelectedInputCurrency}";

    public string DepositDateDisplay => DepositDate.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("it-IT"));

    public string DepositCoverageText => _trip is null
        ? string.Empty
        : $"COPERTURA VERSAMENTI: {CurrencyDisplayService.FormatAmountWithEur(_trip.TotalPaid, _trip)} SU {CurrencyDisplayService.FormatAmountWithEur(_trip.TotalBudget, _trip)}";

    public double DepositCoverageProgress => _trip is null || _trip.TotalBudget <= 0m
        ? 0
        : (double)Math.Min(1m, _trip.TotalPaid / _trip.TotalBudget);

    public IReadOnlyList<CurrencyOption> InputCurrencyOptions =>
        _trip is null
            ? new[] { new CurrencyOption(CurrencyCode.EUR, CurrencyCatalog.GetItalianName(CurrencyCode.EUR)) }
            : CurrencyDisplayService.GetInputOptions(_trip);

    public ICommand AddDepositCommand { get; }
    public ICommand DeleteDepositCommand { get; }
    public ICommand StartEditDepositCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SelectParticipantCommand { get; }
    public ICommand SelectInputCurrencyCommand { get; }
    public ICommand SelectDepositDateCommand { get; }

    public DepositViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Versamenti";
        AddDepositCommand = new Command(async () => await AddDepositAsync());
        DeleteDepositCommand = new Command<DepositHistoryItemViewModel>(async deposit => await DeleteDepositAsync(deposit));
        StartEditDepositCommand = new Command<DepositHistoryItemViewModel>(StartEditDeposit);
        CancelEditCommand = new Command(CancelEdit);
        SelectParticipantCommand = new Command(async () => await SelectParticipantAsync());
        SelectInputCurrencyCommand = new Command(async () => await SelectInputCurrencyAsync());
        SelectDepositDateCommand = new Command(async () => await SelectDepositDateAsync());
    }

    public async Task LoadAsync()
    {
        _trip = await _tripService.GetTripByCodeAsync(_tripCode);
        var deposits = await _tripService.GetDepositsAsync(_tripCode);

        Participants.Clear();
        Deposits.Clear();

        if (_trip is null)
        {
            return;
        }

        foreach (var participant in _trip.Participants.OrderBy(p => p.Name))
        {
            Participants.Add(participant);
        }

        SelectedParticipant = Participants.FirstOrDefault(p => p.Name == SelectedParticipant?.Name) ?? Participants.FirstOrDefault();
        if (!InputCurrencyOptions.Any(option => option.Code == SelectedInputCurrency))
        {
            SelectedInputCurrency = CurrencyCode.EUR;
        }

        foreach (var deposit in deposits.OrderByDescending(d => d.Date).ThenByDescending(d => d.Id))
        {
            var participant = Participants.FirstOrDefault(p => p.Name == deposit.PayerName);
            var totalPaid = deposits
                .Where(d => d.PayerName == deposit.PayerName &&
                            (d.Date < deposit.Date || (d.Date == deposit.Date && d.Id <= deposit.Id)))
                .Sum(d => d.Amount);

            var remainingInEur = participant is null ? 0m : participant.PersonalBudget - totalPaid;

            Deposits.Add(new DepositHistoryItemViewModel
            {
                Id = deposit.Id,
                Date = deposit.Date,
                PayerName = deposit.PayerName,
                AmountInEur = deposit.Amount,
                AmountPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(deposit.Amount, _trip),
                AmountSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(deposit.Amount, _trip),
                RemainingBudgetInEur = remainingInEur,
                RemainingBudgetPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(remainingInEur, _trip),
                RemainingBudgetSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(remainingInEur, _trip)
            });
        }

        OnPropertyChanged(nameof(DepositCoverageText));
        OnPropertyChanged(nameof(DepositCoverageProgress));
        UpdateBudgetPreview();
    }

    private async Task SelectDepositDateAsync()
    {
        var selectedDate = await ShowDatePickerAsync("Data versamento", "Seleziona la data del versamento.", DepositDate);
        if (selectedDate.HasValue)
        {
            DepositDate = selectedDate.Value;
        }
    }

    private async Task SelectParticipantAsync()
    {
        var selected = await ShowSelectionAsync(
            "Partecipante",
            "Seleziona il partecipante che sta versando.",
            Participants.ToList(),
            participant => participant.Name.ToUpperInvariant(),
            SelectedParticipant);

        if (selected is not null)
        {
            SelectedParticipant = selected;
        }
    }

    private async Task SelectInputCurrencyAsync()
    {
        if (_trip is null)
        {
            return;
        }

        var selected = await ShowSelectionAsync(
            "Valuta di inserimento",
            "Puoi registrare il versamento in EUR oppure nella valuta del viaggio. Il sistema salva in EUR e converte automaticamente.",
            InputCurrencyOptions,
            option => option.Label,
            InputCurrencyOptions.FirstOrDefault(option => option.Code == SelectedInputCurrency));

        if (selected is not null)
        {
            SelectedInputCurrency = selected.Code;
        }
    }

    private async Task AddDepositAsync()
    {
        if (_trip is null)
        {
            await ShowAlertAsync("Viaggio non trovato", "Impossibile caricare il viaggio.");
            return;
        }

        if (SelectedParticipant is null)
        {
            await ShowAlertAsync("Partecipante mancante", "Seleziona chi ha effettuato il versamento.");
            return;
        }

        if (!TryParseDecimalInput(AmountInput, out var inputAmount) || inputAmount <= 0)
        {
            await ShowAlertAsync("Importo non valido", "Inserisci un importo maggiore di zero.");
            return;
        }

        var participant = _trip.Participants.FirstOrDefault(p => p.Name == SelectedParticipant.Name);
        if (participant is null)
        {
            await ShowAlertAsync("Partecipante mancante", "Impossibile caricare il partecipante selezionato.");
            return;
        }

        var amountInEur = CurrencyDisplayService.ConvertInputToEur(inputAmount, SelectedInputCurrency, _trip);
        var currentPaid = participant.Deposits
            .Where(d => !_editingDepositId.HasValue || d.Id != _editingDepositId.Value)
            .Sum(d => d.Amount);
        var remainingBudget = participant.PersonalBudget - currentPaid;
        var allowBudgetIncrease = false;

        if (amountInEur > remainingBudget)
        {
            allowBudgetIncrease = await ShowConfirmAsync(
                "Aumenta budget",
                $"Il versamento convertito supera il residuo di {SelectedParticipant.Name}. Residuo attuale: {CurrencyDisplayService.FormatAmountWithEur(remainingBudget, _trip)}. Vuoi aumentare il budget per tutti i partecipanti?");

            if (!allowBudgetIncrease)
            {
                return;
            }
        }

        await RunBusyAsync(async () =>
        {
            if (_editingDepositId.HasValue)
            {
                await _tripService.UpdateDepositAsync(_tripCode, _editingDepositId.Value, SelectedParticipant.Name, DepositDate, amountInEur, allowBudgetIncrease);
            }
            else
            {
                await _tripService.AddDepositAsync(_tripCode, SelectedParticipant.Name, DepositDate, amountInEur, allowBudgetIncrease);
            }

            CancelEdit();
            await LoadAsync();
        });
    }

    private async Task DeleteDepositAsync(DepositHistoryItemViewModel? deposit)
    {
        if (deposit is null)
        {
            return;
        }

        var confirmed = await ShowConfirmAsync("Elimina versamento", $"Vuoi eliminare il versamento di {deposit.PayerName} pari a {deposit.AmountPrimaryDisplay}{deposit.AmountSecondaryDisplay}?");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _tripService.DeleteDepositAsync(_tripCode, deposit.Id);
            await LoadAsync();
        });
    }

    private void StartEditDeposit(DepositHistoryItemViewModel? deposit)
    {
        if (deposit is null)
        {
            return;
        }

        _editingDepositId = deposit.Id;
        SelectedParticipant = Participants.FirstOrDefault(p => p.Name == deposit.PayerName);
        SelectedInputCurrency = CurrencyCode.EUR;
        AmountInput = FormatDecimalInput(deposit.AmountInEur, "0.00##");
        DepositDate = deposit.Date;
        SubmitButtonText = "Salva modifiche";
        IsEditing = true;
        UpdateBudgetPreview();
    }

    private void CancelEdit()
    {
        _editingDepositId = null;
        SelectedInputCurrency = CurrencyCode.EUR;
        AmountInput = string.Empty;
        DepositDate = DateTime.Today;
        SubmitButtonText = "Registra versamento";
        IsEditing = false;
        UpdateBudgetPreview();
    }

    private void UpdateBudgetPreview()
    {
        if (_trip is null || SelectedParticipant is null)
        {
            BudgetPreviewText = "Seleziona un partecipante per vedere il residuo disponibile.";
            return;
        }

        var participant = _trip.Participants.FirstOrDefault(p => p.Name == SelectedParticipant.Name) ?? SelectedParticipant;
        var alreadyPaid = participant.Deposits
            .Where(d => !_editingDepositId.HasValue || d.Id != _editingDepositId.Value)
            .Sum(d => d.Amount);
        var remaining = participant.PersonalBudget - alreadyPaid;
        var projected = remaining;

        if (TryParseDecimalInput(AmountInput, out var parsedAmount) && parsedAmount > 0)
        {
            var amountInEur = CurrencyDisplayService.ConvertInputToEur(parsedAmount, SelectedInputCurrency, _trip);
            projected = remaining - amountInEur;
        }

        BudgetPreviewText =
            $"Residuo attuale di {SelectedParticipant.Name}: {CurrencyDisplayService.FormatAmountWithEur(remaining, _trip)}. " +
            $"Residuo dopo questo versamento: {CurrencyDisplayService.FormatAmountWithEur(projected, _trip)}.";
    }

    private void ConvertAmountInputBetweenModes(CurrencyCode previousCurrency, CurrencyCode newCurrency)
    {
        if (_trip is null || !TryParseDecimalInput(AmountInput, out var currentAmount) || currentAmount <= 0 || previousCurrency == newCurrency)
        {
            return;
        }

        var amountInEur = CurrencyDisplayService.ConvertInputToEur(currentAmount, previousCurrency, _trip);
        var converted = newCurrency == CurrencyCode.EUR
            ? amountInEur
            : CurrencyDisplayService.ConvertEurToTripCurrency(amountInEur, _trip);

        AmountInput = FormatDecimalInput(converted, "0.00##");
    }
}
