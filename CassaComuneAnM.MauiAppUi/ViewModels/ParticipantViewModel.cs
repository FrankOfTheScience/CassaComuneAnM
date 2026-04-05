using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ParticipantViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly string _tripCode;
    private Trip? _trip;
    private string _participantName = string.Empty;
    private decimal _personalBudget;

    public ObservableCollection<Participant> Participants { get; } = new();

    public string ParticipantName
    {
        get => _participantName;
        set => SetProperty(ref _participantName, value);
    }

    public decimal PersonalBudget
    {
        get => _personalBudget;
        set => SetProperty(ref _personalBudget, value);
    }

    public ICommand AddParticipantCommand { get; }
    public ICommand RemoveParticipantCommand { get; }

    public ParticipantViewModel(ITripService tripService, string tripCode)
    {
        _tripService = tripService;
        _tripCode = tripCode;
        Title = "Partecipanti";

        AddParticipantCommand = new Command(async () => await AddParticipantAsync());
        RemoveParticipantCommand = new Command<Participant>(async participant => await RemoveParticipantAsync(participant));
    }

    public async Task LoadAsync()
    {
        _trip = await _tripService.GetTripByCodeAsync(_tripCode);
        Participants.Clear();

        if (_trip is null)
        {
            return;
        }

        foreach (var participant in _trip.Participants.OrderBy(p => p.Name))
        {
            Participants.Add(participant);
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

        var participant = new Participant
        {
            Name = ParticipantName.Trim(),
            Balance = 0m,
            PersonalBudget = PersonalBudget,
            TripId = _trip.Id,
            Trip = _trip
        };

        await _tripService.AddParticipantAsync(_tripCode, participant);
        ParticipantName = string.Empty;
        PersonalBudget = 0m;
        await LoadAsync();
    }

    private async Task RemoveParticipantAsync(Participant? participant)
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

        await _tripService.RemoveParticipantAsync(_tripCode, participant.Name);
        await LoadAsync();
    }
}
