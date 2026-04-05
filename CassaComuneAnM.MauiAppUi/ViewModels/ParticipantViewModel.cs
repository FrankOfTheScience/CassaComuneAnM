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
    private Trip? _trip;
    private string _participantName = string.Empty;
    private string _personalBudgetInput = string.Empty;

    public ObservableCollection<ParticipantListItemViewModel> Participants { get; } = new();

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

    public string PersonalBudgetPlaceholder =>
        "Budget personale in EUR";

    public string PersonalBudgetHelpText =>
        _trip?.BudgetPerPax > 0
            ? $"Vuoto = usa il budget base viaggio ({_trip.BudgetPerPax:F2} EUR)."
            : "Inserisci il budget personale del partecipante.";

    public ICommand AddParticipantCommand { get; }
    public ICommand RemoveParticipantCommand { get; }

    public ParticipantViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Partecipanti";

        AddParticipantCommand = new Command(async () => await AddParticipantAsync());
        RemoveParticipantCommand = new Command<ParticipantListItemViewModel>(async participant => await RemoveParticipantAsync(participant));
    }

    public async Task LoadAsync()
    {
        _trip = await _tripService.GetTripByCodeAsync(_tripCode);
        Participants.Clear();
        OnPropertyChanged(nameof(PersonalBudgetPlaceholder));

        if (_trip is null)
        {
            return;
        }

        foreach (var participant in _trip.Participants.OrderBy(p => p.Name))
        {
            Participants.Add(new ParticipantListItemViewModel
            {
                Name = participant.Name,
                ParticipantName = participant.Name,
                BudgetPrimaryDisplay = CurrencyDisplayService.FormatPrimaryAmount(participant.PersonalBudget, _trip),
                BudgetSecondaryDisplay = CurrencyDisplayService.FormatSecondaryEurAmount(participant.PersonalBudget, _trip)
            });
        }
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

        if (_trip.Participants.Any(p => string.Equals(p.Name, ParticipantName.Trim(), StringComparison.OrdinalIgnoreCase)))
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
}
