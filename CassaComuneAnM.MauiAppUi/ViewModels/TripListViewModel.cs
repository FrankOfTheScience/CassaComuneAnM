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
    private readonly List<Trip> _allTrips = new();
    private string _searchText = string.Empty;
    private string _selectedSortOption = "DATA";
    private bool _sortDescending = true;

    public ObservableCollection<Trip> Trips { get; } = new();
    public IReadOnlyList<string> SortOptions { get; } = new[] { "DATA", "NOME", "CODICE", "PAESE" };

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

    public ICommand OpenTripCommand { get; }
    public ICommand CreateTripCommand { get; }
    public ICommand SelectSortCommand { get; }
    public ICommand ToggleSortDirectionCommand { get; }

    public TripListViewModel(ITripService tripService, IServiceProvider serviceProvider)
    {
        _tripService = tripService;
        _serviceProvider = serviceProvider;
        Title = "Viaggi";

        OpenTripCommand = new Command<Trip>(async trip => await OpenTripAsync(trip));
        CreateTripCommand = new Command(async () => await CreateTripAsync());
        SelectSortCommand = new Command(async () => await SelectSortAsync());
        ToggleSortDirectionCommand = new Command(ToggleSortDirection);
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
            _allTrips.Clear();
            _allTrips.AddRange(trips);
            ApplyFilters();
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

    private async Task SelectSortAsync()
    {
        var selected = await ShowSelectionAsync(
            "Ordina viaggi",
            "Scegli il campo con cui ordinare l'elenco viaggi.",
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
        IEnumerable<Trip> query = _allTrips;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(trip =>
                trip.TripName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                trip.TripCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                trip.Country.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        query = _selectedSortOption switch
        {
            "NOME" => _sortDescending ? query.OrderByDescending(trip => trip.TripName) : query.OrderBy(trip => trip.TripName),
            "CODICE" => _sortDescending ? query.OrderByDescending(trip => trip.TripCode) : query.OrderBy(trip => trip.TripCode),
            "PAESE" => _sortDescending ? query.OrderByDescending(trip => trip.Country) : query.OrderBy(trip => trip.Country),
            _ => _sortDescending ? query.OrderByDescending(trip => trip.TripDate) : query.OrderBy(trip => trip.TripDate)
        };

        Trips.Clear();
        foreach (var trip in query)
        {
            Trips.Add(trip);
        }
    }
}
