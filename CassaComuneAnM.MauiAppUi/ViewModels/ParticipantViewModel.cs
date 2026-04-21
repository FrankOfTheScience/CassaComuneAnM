using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.MauiAppUi.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ParticipantViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private readonly List<ParticipantListItemViewModel> _allParticipants = new();
    private Trip? _trip;
    private string _participantName = string.Empty;
    private string _personalBudgetInput = string.Empty;
    private string _searchText = string.Empty;
    private string _selectedSortOption = "NOME";
    private bool _sortDescending;

    public ObservableCollection<ParticipantListItemViewModel> Participants { get; } = new();
    public IReadOnlyList<string> SortOptions { get; } = new[] { "NOME", "BUDGET" };

    public string ParticipantName
    {
        get => _participantName;
        set => SetProperty(ref _participantName, value);
    }

    public string PersonalBudgetInput
    {
        get => _personalBudgetInput;
        set => SetProperty(ref _personalBudgetInput, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    public string SelectedSortOption => _selectedSortOption;
    public string SortDirectionLabel => _sortDescending ? "DESC" : "ASC";

    public string PersonalBudgetPlaceholder => "Budget personale in EUR";

    public string PersonalBudgetHelpText =>
        _trip?.BudgetPerPax > 0
            ? $"Inserisci il budget personale del partecipante. Se lasci vuoto, usa il budget base viaggio ({_trip.BudgetPerPax:F2} EUR)."
            : "Inserisci il budget personale del partecipante. Se lasci vuoto, si applica il budget standard del viaggio.";

    public ICommand AddParticipantCommand { get; }
    public ICommand RemoveParticipantCommand { get; }
    public ICommand SelectSortCommand { get; }
    public ICommand ToggleSortDirectionCommand { get; }
    public ICommand ShowParticipantDetailCommand { get; }

    public ParticipantViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Partecipanti";

        AddParticipantCommand = new Command(async () => await AddParticipantAsync());
        RemoveParticipantCommand = new Command<ParticipantListItemViewModel>(async participant => await RemoveParticipantAsync(participant));
        SelectSortCommand = new Command(async () => await SelectSortAsync());
        ToggleSortDirectionCommand = new Command(ToggleSortDirection);
        ShowParticipantDetailCommand = new Command<ParticipantListItemViewModel>(async participant => await ShowParticipantDetailAsync(participant));
    }

    public async Task LoadAsync()
    {
        _trip = await _tripService.GetTripByCodeAsync(_tripCode);
        Participants.Clear();
        _allParticipants.Clear();
        OnPropertyChanged(nameof(PersonalBudgetPlaceholder));
        OnPropertyChanged(nameof(PersonalBudgetHelpText));

        if (_trip is null)
        {
            return;
        }

        foreach (var participant in (_trip.Participants ?? new List<Participant>()).OrderBy(p => p.Name ?? string.Empty))
        {
            _allParticipants.Add(new ParticipantListItemViewModel
            {
                Name = participant.Name ?? string.Empty,
                ParticipantName = participant.Name ?? string.Empty,
                BudgetInEur = participant.PersonalBudget,
                BudgetPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(participant.PersonalBudget, _trip),
                BudgetSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(participant.PersonalBudget, _trip)
            });
        }

        ApplyFilters();
    }

    private async Task AddParticipantAsync()
    {
        if (_trip is null)
        {
            await LoadAsync();
        }

        if (_trip is null)
        {
            await ShowAlertAsync("Viaggio non trovato", "Impossibile caricare il viaggio.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ParticipantName))
        {
            await ShowAlertAsync("Nome mancante", "Inserisci il nome del partecipante.");
            return;
        }

        if ((_trip.Participants ?? new List<Participant>()).Any(p => string.Equals(p.Name, ParticipantName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            await ShowAlertAsync("Duplicato", "Esiste già un partecipante con questo nome.");
            return;
        }

        var personalBudget = TryParseDecimalInput(PersonalBudgetInput, out var parsedBudget)
            ? parsedBudget
            : _trip.BudgetPerPax;

        var participant = new Participant
        {
            Name = ParticipantName.Trim(),
            Balance = 0m,
            PersonalBudget = personalBudget,
            TripId = _trip.Id,
            Trip = _trip
        };

        await RunBusyAsync(async () =>
        {
            await _tripService.AddParticipantAsync(_tripCode, participant);
            ParticipantName = string.Empty;
            PersonalBudgetInput = string.Empty;
            await LoadAsync();
        });
    }

    private async Task RemoveParticipantAsync(ParticipantListItemViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        var confirmed = await ShowConfirmAsync("Rimuovi partecipante", $"Vuoi rimuovere {participant.Name}?");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _tripService.RemoveParticipantAsync(_tripCode, participant.ParticipantName);
            await LoadAsync();
        });
    }

    private async Task ShowParticipantDetailAsync(ParticipantListItemViewModel? participant)
    {
        if (participant is null)
        {
            return;
        }

        var action = await ShowDetailActionsAsync(
            participant.Name,
            new[]
            {
                new DialogDetailRow("Budget", participant.BudgetPrimaryDisplay, participant.BudgetSecondaryDisplay)
            },
            new[] { "RIMUOVI" });

        if (action == "RIMUOVI")
        {
            await RemoveParticipantAsync(participant);
        }
    }

    private async Task SelectSortAsync()
    {
        var selected = await ShowSelectionAsync(
            "Ordina partecipanti",
            "Scegli come ordinare la lista partecipanti.",
            SortOptions,
            option => option,
            _selectedSortOption);

        if (!string.IsNullOrWhiteSpace(selected))
        {
            _selectedSortOption = selected;
            OnPropertyChanged(nameof(SelectedSortOption));
            ApplyFilters();
        }
    }

    private void ToggleSortDirection()
    {
        _sortDescending = !_sortDescending;
        OnPropertyChanged(nameof(SortDirectionLabel));
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        IEnumerable<ParticipantListItemViewModel> query = _allParticipants;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(participant => participant.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        query = _selectedSortOption switch
        {
            "BUDGET" => _sortDescending ? query.OrderByDescending(participant => participant.BudgetInEur) : query.OrderBy(participant => participant.BudgetInEur),
            _ => _sortDescending ? query.OrderByDescending(participant => participant.Name) : query.OrderBy(participant => participant.Name)
        };

        Participants.Clear();
        foreach (var participant in query)
        {
            Participants.Add(participant);
        }
    }
}
