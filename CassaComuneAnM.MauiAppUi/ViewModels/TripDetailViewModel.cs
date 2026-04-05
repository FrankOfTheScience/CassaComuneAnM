using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
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

    public ObservableCollection<ParticipantSummaryViewModel> ParticipantSummaries { get; } = new();

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

        await _tripService.DeleteTripAsync(_tripCode);
        await Navigation!.PopAsync();
    }

    private void StartTripEdit()
    {
        if (Trip is null)
        {
            return;
        }

        IsEditingTrip = true;
    }

    private void CancelTripEdit()
    {
        IsEditingTrip = false;
    }

    private async Task SaveTripAsync()
    {
        if (Trip is null)
        {
            return;
        }

        await _tripService.SaveOrUpdateTripAsync(Trip);
        IsEditingTrip = false;
        await LoadTripAsync();
    }
}
