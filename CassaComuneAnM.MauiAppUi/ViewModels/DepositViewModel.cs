using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class DepositViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private Participant? _selectedParticipant;
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
                UpdateBudgetPreview();
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
                UpdateBudgetPreview();
            }
        }
    }

    public DateTime DepositDate
    {
        get => _depositDate;
        set => SetProperty(ref _depositDate, value);
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

    public ICommand AddDepositCommand { get; }
    public ICommand DeleteDepositCommand { get; }
    public ICommand StartEditDepositCommand { get; }
    public ICommand CancelEditCommand { get; }

    public DepositViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Versamenti";
        AddDepositCommand = new Command(async () => await AddDepositAsync());
        DeleteDepositCommand = new Command<DepositHistoryItemViewModel>(async deposit => await DeleteDepositAsync(deposit));
        StartEditDepositCommand = new Command<DepositHistoryItemViewModel>(StartEditDeposit);
        CancelEditCommand = new Command(CancelEdit);
    }

    public async Task LoadAsync()
    {
        var trip = await _tripService.GetTripByCodeAsync(_tripCode);
        var deposits = await _tripService.GetDepositsAsync(_tripCode);

        Participants.Clear();
        Deposits.Clear();

        if (trip is null)
        {
            return;
        }

        foreach (var participant in trip.Participants.OrderBy(p => p.Name))
        {
            Participants.Add(participant);
        }

        SelectedParticipant = Participants.FirstOrDefault(p => p.Name == SelectedParticipant?.Name) ?? Participants.FirstOrDefault();

        foreach (var deposit in deposits.OrderByDescending(d => d.Date).ThenByDescending(d => d.Id))
        {
            var participant = Participants.FirstOrDefault(p => p.Name == deposit.PayerName);
            var totalPaid = deposits
                .Where(d => d.PayerName == deposit.PayerName &&
                            (d.Date < deposit.Date || (d.Date == deposit.Date && d.Id <= deposit.Id)))
                .Sum(d => d.Amount);

            Deposits.Add(new DepositHistoryItemViewModel
            {
                Id = deposit.Id,
                Date = deposit.Date,
                PayerName = deposit.PayerName,
                Amount = deposit.Amount,
                RemainingBudget = participant is null ? 0m : participant.PersonalBudget - totalPaid
            });
        }

        UpdateBudgetPreview();
    }

    private async Task AddDepositAsync()
    {
        if (SelectedParticipant is null)
        {
            await ShowAlertAsync("Partecipante mancante", "Seleziona chi ha effettuato il versamento.");
            return;
        }

        if (!TryParseDecimalInput(AmountInput, out var amount) || amount <= 0)
        {
            await ShowAlertAsync("Importo non valido", "Inserisci un importo maggiore di zero.");
            return;
        }

        var trip = await _tripService.GetTripByCodeAsync(_tripCode);
        var participant = trip?.Participants.FirstOrDefault(p => p.Name == SelectedParticipant.Name);
        if (participant is null)
        {
            await ShowAlertAsync("Partecipante mancante", "Impossibile caricare il partecipante selezionato.");
            return;
        }

        var currentPaid = participant.Deposits
            .Where(d => !_editingDepositId.HasValue || d.Id != _editingDepositId.Value)
            .Sum(d => d.Amount);
        var remainingBudget = participant.PersonalBudget - currentPaid;
        var allowBudgetIncrease = false;

        if (amount > remainingBudget)
        {
            allowBudgetIncrease = await ShowConfirmAsync(
                "Aumenta budget",
                $"Il versamento supera il residuo di {SelectedParticipant.Name} ({remainingBudget:F2}). Vuoi aumentare il budget per tutti i partecipanti?");

            if (!allowBudgetIncrease)
            {
                return;
            }
        }

        await RunBusyAsync(async () =>
        {
            if (_editingDepositId.HasValue)
            {
                await _tripService.UpdateDepositAsync(_tripCode, _editingDepositId.Value, SelectedParticipant.Name, DepositDate, amount, allowBudgetIncrease);
            }
            else
            {
                await _tripService.AddDepositAsync(_tripCode, SelectedParticipant.Name, DepositDate, amount, allowBudgetIncrease);
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

        var confirmed = await ShowConfirmAsync("Elimina versamento", $"Vuoi eliminare il versamento di {deposit.PayerName}?");
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
        AmountInput = FormatDecimalInput(deposit.Amount, "0.00##");
        DepositDate = deposit.Date;
        SubmitButtonText = "Salva modifiche";
        IsEditing = true;
        UpdateBudgetPreview();
    }

    private void CancelEdit()
    {
        _editingDepositId = null;
        AmountInput = string.Empty;
        DepositDate = DateTime.Today;
        SubmitButtonText = "Registra versamento";
        IsEditing = false;
        UpdateBudgetPreview();
    }

    private void UpdateBudgetPreview()
    {
        if (SelectedParticipant is null)
        {
            BudgetPreviewText = "Seleziona un partecipante per vedere il residuo disponibile.";
            return;
        }

        var alreadyPaid = SelectedParticipant.Deposits
            .Where(d => !_editingDepositId.HasValue || d.Id != _editingDepositId.Value)
            .Sum(d => d.Amount);
        var remaining = SelectedParticipant.PersonalBudget - alreadyPaid;
        var projected = TryParseDecimalInput(AmountInput, out var amount)
            ? remaining - amount
            : remaining;

        BudgetPreviewText =
            $"Residuo attuale di {SelectedParticipant.Name}: EUR {remaining:F2}. " +
            $"Residuo dopo questo versamento: EUR {projected:F2}.";
    }
}
