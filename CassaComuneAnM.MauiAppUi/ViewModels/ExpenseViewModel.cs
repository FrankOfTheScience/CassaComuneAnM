using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using CassaComuneAnM.MauiAppUi.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ExpenseViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private Trip? _trip;
    private string _helperText = "Seleziona chi non beneficia della spesa. Se non selezioni nessuno, la spesa viene ripartita su tutto il gruppo.";
    private string _description = string.Empty;
    private CurrencyCode _selectedInputCurrency = CurrencyCode.EUR;
    private string _amountInput = string.Empty;
    private DateTime _expenseDate = DateTime.Today;
    private bool _tourLeaderFree;
    private int? _editingExpenseId;
    private string _submitButtonText = "Registra spesa";
    private bool _isEditing;

    public ObservableCollection<SelectableParticipantViewModel> Participants { get; } = new();
    public ObservableCollection<ExpenseHistoryItemViewModel> Expenses { get; } = new();

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                UpdateHelperText();
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
            UpdateHelperText();
        }
    }

    public string AmountInput
    {
        get => _amountInput;
        set
        {
            if (SetProperty(ref _amountInput, value))
            {
                UpdateHelperText();
            }
        }
    }

    public DateTime ExpenseDate
    {
        get => _expenseDate;
        set => SetProperty(ref _expenseDate, value);
    }

    public bool TourLeaderFree
    {
        get => _tourLeaderFree;
        set
        {
            if (SetProperty(ref _tourLeaderFree, value))
            {
                UpdateHelperText();
            }
        }
    }

    public string HelperText
    {
        get => _helperText;
        set => SetProperty(ref _helperText, value);
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

    public string SelectedInputCurrencyLabel =>
        _trip is null
            ? "INSERIMENTO IN EUR"
            : CurrencyDisplayService.FormatInputModeLabel(SelectedInputCurrency, _trip);

    public string AmountPlaceholder =>
        SelectedInputCurrency == CurrencyCode.EUR
            ? "Importo spesa in EUR"
            : $"Importo spesa in {SelectedInputCurrency}";

    public IReadOnlyList<CurrencyOption> InputCurrencyOptions =>
        _trip is null
            ? new[] { new CurrencyOption(CurrencyCode.EUR, CurrencyCatalog.GetItalianName(CurrencyCode.EUR)) }
            : CurrencyDisplayService.GetInputOptions(_trip);

    public ICommand AddExpenseCommand { get; }
    public ICommand DeleteExpenseCommand { get; }
    public ICommand StartEditExpenseCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SelectInputCurrencyCommand { get; }

    public ExpenseViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Spese";
        AddExpenseCommand = new Command(async () => await AddExpenseAsync());
        DeleteExpenseCommand = new Command<ExpenseHistoryItemViewModel>(async expense => await DeleteExpenseAsync(expense));
        StartEditExpenseCommand = new Command<ExpenseHistoryItemViewModel>(StartEditExpense);
        CancelEditCommand = new Command(CancelEdit);
        SelectInputCurrencyCommand = new Command(async () => await SelectInputCurrencyAsync());
    }

    public async Task LoadAsync()
    {
        _trip = await _tripService.GetTripByCodeAsync(_tripCode);
        var expenses = await _tripService.GetExpensesAsync(_tripCode);

        Participants.Clear();
        Expenses.Clear();

        if (_trip is null)
        {
            return;
        }

        foreach (var participant in _trip.Participants.OrderBy(p => p.Name))
        {
            var selectable = new SelectableParticipantViewModel { Name = participant.Name };
            selectable.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(SelectableParticipantViewModel.IsSelected))
                {
                    UpdateHelperText();
                }
            };
            Participants.Add(selectable);
        }

        foreach (var expense in expenses.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id))
        {
            var beneficiaries = expense.ExpenseParticipants
                .Select(ep => ep.Participant?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            Expenses.Add(new ExpenseHistoryItemViewModel
            {
                Id = expense.Id,
                Date = expense.Date,
                Description = expense.Description,
                AmountInEur = expense.Amount,
                AmountPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(expense.Amount, _trip),
                AmountSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(expense.Amount, _trip),
                BeneficiariesText = beneficiaries.Count > 0 ? string.Join(", ", beneficiaries) : "N/D"
            });
        }

        UpdateHelperText();
    }

    private async Task SelectInputCurrencyAsync()
    {
        if (_trip is null)
        {
            return;
        }

        var selected = await ShowSelectionAsync(
            "Valuta di inserimento",
            "Puoi registrare la spesa in EUR oppure nella valuta del viaggio. Il sistema salva in EUR e converte automaticamente.",
            InputCurrencyOptions,
            option => option.Label,
            InputCurrencyOptions.FirstOrDefault(option => option.Code == SelectedInputCurrency));

        if (selected is not null)
        {
            SelectedInputCurrency = selected.Code;
        }
    }

    private async Task AddExpenseAsync()
    {
        if (_trip is null)
        {
            await ShowAlertAsync("Viaggio non trovato", "Impossibile caricare il viaggio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            await ShowAlertAsync("Descrizione mancante", "Inserisci la descrizione della spesa.");
            return;
        }

        if (!TryParseDecimalInput(AmountInput, out var inputAmount) || inputAmount <= 0)
        {
            await ShowAlertAsync("Importo non valido", "Inserisci un importo maggiore di zero.");
            return;
        }

        var amountInEur = CurrencyDisplayService.ConvertInputToEur(inputAmount, SelectedInputCurrency, _trip);
        var excludedNames = Participants.Where(p => p.IsSelected).Select(p => p.Name).ToList();

        await RunBusyAsync(async () =>
        {
            if (_editingExpenseId.HasValue)
            {
                await _tripService.UpdateExpenseAsync(_tripCode, _editingExpenseId.Value, ExpenseDate, Description.Trim(), amountInEur, TourLeaderFree, excludedNames);
            }
            else
            {
                await _tripService.AddExpenseAsync(_tripCode, ExpenseDate, Description.Trim(), amountInEur, TourLeaderFree, excludedNames);
            }

            CancelEdit();
            await LoadAsync();

            if (_trip?.CashBalance < 0m)
            {
                await ShowAlertAsync(
                    "Cassa in negativo",
                    $"La spesa è stata registrata, ma il saldo cassa è ora negativo di {CurrencyDisplayService.FormatAmountWithEur(Math.Abs(_trip.CashBalance), _trip)}. Registra o copri il disavanzo il prima possibile.");
            }
        });
    }

    private async Task DeleteExpenseAsync(ExpenseHistoryItemViewModel? expense)
    {
        if (expense is null)
        {
            return;
        }

        var confirmed = await ShowConfirmAsync("Elimina spesa", $"Vuoi eliminare '{expense.Description}' pari a {expense.AmountPrimaryDisplay}{expense.AmountSecondaryDisplay}?");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _tripService.DeleteExpenseAsync(_tripCode, expense.Id);
            await LoadAsync();
        });
    }

    private void StartEditExpense(ExpenseHistoryItemViewModel? expense)
    {
        if (expense is null)
        {
            return;
        }

        _editingExpenseId = expense.Id;
        Description = expense.Description;
        SelectedInputCurrency = CurrencyCode.EUR;
        AmountInput = FormatDecimalInput(expense.AmountInEur, "0.00##");
        ExpenseDate = expense.Date;
        TourLeaderFree = expense.Description.Contains("TL Free", StringComparison.OrdinalIgnoreCase);
        SubmitButtonText = "Salva modifiche";
        IsEditing = true;

        var selectedNames = expense.BeneficiariesText
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var participant in Participants)
        {
            participant.IsSelected = !selectedNames.Contains(participant.Name);
        }
    }

    private void CancelEdit()
    {
        _editingExpenseId = null;
        Description = string.Empty;
        SelectedInputCurrency = CurrencyCode.EUR;
        AmountInput = string.Empty;
        ExpenseDate = DateTime.Today;
        TourLeaderFree = false;
        SubmitButtonText = "Registra spesa";
        IsEditing = false;

        foreach (var participant in Participants)
        {
            participant.IsSelected = false;
        }

        UpdateHelperText();
    }

    private void UpdateHelperText()
    {
        var participantCount = Participants.Count;
        var excludedCount = Participants.Count(p => p.IsSelected);
        var beneficiaries = Math.Max(0, participantCount - excludedCount);

        if (_trip is null || participantCount == 0)
        {
            HelperText = "Aggiungi prima dei partecipanti al viaggio per poter registrare una spesa.";
            return;
        }

        if (beneficiaries == 0)
        {
            HelperText = "Almeno un partecipante deve beneficiare della spesa.";
            return;
        }

        if (!TryParseDecimalInput(AmountInput, out var inputAmount) || inputAmount <= 0)
        {
            HelperText = "Inserisci l'importo in EUR o nella valuta del viaggio. Il sistema converte e salva sempre in EUR.";
            return;
        }

        var amountInEur = CurrencyDisplayService.ConvertInputToEur(inputAmount, SelectedInputCurrency, _trip);
        var amountPerHead = amountInEur / participantCount;
        var tlSuffix = TourLeaderFree ? " Modalità tour leader gratuito attiva." : string.Empty;
        HelperText =
            $"Importo registrato: {CurrencyDisplayService.FormatAmountWithEur(amountInEur, _trip)}. " +
            $"Beneficiari: {beneficiaries}/{participantCount}. Quota base per testa: {CurrencyDisplayService.FormatAmountWithEur(amountPerHead, _trip)}.{tlSuffix}";
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
