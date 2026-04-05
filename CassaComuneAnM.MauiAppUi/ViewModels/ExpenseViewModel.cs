using CassaComuneAnm.Application.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ExpenseViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private string _helperText = "Seleziona chi non beneficia della spesa. Se non selezioni nessuno, la spesa viene ripartita su tutto il gruppo.";
    private string _description = string.Empty;
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

    public ICommand AddExpenseCommand { get; }
    public ICommand DeleteExpenseCommand { get; }
    public ICommand StartEditExpenseCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ExpenseViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Spese";
        AddExpenseCommand = new Command(async () => await AddExpenseAsync());
        DeleteExpenseCommand = new Command<ExpenseHistoryItemViewModel>(async expense => await DeleteExpenseAsync(expense));
        StartEditExpenseCommand = new Command<ExpenseHistoryItemViewModel>(StartEditExpense);
        CancelEditCommand = new Command(CancelEdit);
    }

    public async Task LoadAsync()
    {
        var trip = await _tripService.GetTripByCodeAsync(_tripCode);
        var expenses = await _tripService.GetExpensesAsync(_tripCode);

        Participants.Clear();
        Expenses.Clear();

        if (trip is null)
        {
            return;
        }

        foreach (var participant in trip.Participants.OrderBy(p => p.Name))
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
                Amount = expense.Amount,
                BeneficiariesText = beneficiaries.Count > 0 ? string.Join(", ", beneficiaries) : "N/D"
            });
        }
    }

    private async Task AddExpenseAsync()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            await ShowAlertAsync("Descrizione mancante", "Inserisci la descrizione della spesa.");
            return;
        }

        if (!TryParseDecimalInput(AmountInput, out var amount) || amount <= 0)
        {
            await ShowAlertAsync("Importo non valido", "Inserisci un importo maggiore di zero.");
            return;
        }

        var excludedNames = Participants.Where(p => p.IsSelected).Select(p => p.Name).ToList();

        await RunBusyAsync(async () =>
        {
            if (_editingExpenseId.HasValue)
            {
                await _tripService.UpdateExpenseAsync(_tripCode, _editingExpenseId.Value, ExpenseDate, Description.Trim(), amount, TourLeaderFree, excludedNames);
            }
            else
            {
                await _tripService.AddExpenseAsync(_tripCode, ExpenseDate, Description.Trim(), amount, TourLeaderFree, excludedNames);
            }

            CancelEdit();
            await LoadAsync();
        });
    }

    private async Task DeleteExpenseAsync(ExpenseHistoryItemViewModel? expense)
    {
        if (expense is null)
        {
            return;
        }

        var confirmed = await ShowConfirmAsync("Elimina spesa", $"Vuoi eliminare '{expense.Description}'?");
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
        AmountInput = FormatDecimalInput(expense.Amount, "0.00##");
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

        if (participantCount == 0)
        {
            HelperText = "Aggiungi prima dei partecipanti al viaggio per poter registrare una spesa.";
            return;
        }

        if (beneficiaries == 0)
        {
            HelperText = "Almeno un partecipante deve beneficiare della spesa.";
            return;
        }

        var amountPerHead = TryParseDecimalInput(AmountInput, out var amount) && amount > 0
            ? amount / participantCount
            : 0m;
        var tlSuffix = TourLeaderFree ? " Modalità tour leader gratuito attiva." : string.Empty;
        HelperText = $"Beneficiari: {beneficiaries}/{participantCount}. Quota base per testa: EUR {amountPerHead:F2}.{tlSuffix}";
    }
}
