using CassaComuneAnM.MauiAppUi.ViewModels;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class TripListPage : ContentPage
{
    private readonly TripListViewModel _viewModel;

    public TripListPage(TripListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTripsAsync();
    }
}
