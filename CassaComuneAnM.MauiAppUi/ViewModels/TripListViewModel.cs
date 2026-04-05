using CassaComuneAnm.Application.Interfaces;
using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.MauiAppUi.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class TripListViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<Trip> Trips { get; } = new();

    public ICommand OpenTripCommand { get; }
    public ICommand CreateTripCommand { get; }

    public TripListViewModel(ITripService tripService, IServiceProvider serviceProvider)
    {
        _tripService = tripService;
        _serviceProvider = serviceProvider;
        Title = "Viaggi";

        OpenTripCommand = new Command<Trip>(async trip => await OpenTripAsync(trip));
        CreateTripCommand = new Command(async () => await CreateTripAsync());
    }

    public async Task LoadTripsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var trips = await _tripService.GetAllTripsAsync();
            Trips.Clear();

            foreach (var trip in trips.OrderByDescending(t => t.TripDate))
            {
                Trips.Add(trip);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenTripAsync(Trip? trip)
    {
        if (trip is null)
        {
            return;
        }

        await Navigation!.PushAsync(new TripDetailPage(_serviceProvider, trip.TripCode));
    }

    private async Task CreateTripAsync()
    {
        await Navigation!.PushAsync(_serviceProvider.GetRequiredService<CreateTripPage>());
    }
}
