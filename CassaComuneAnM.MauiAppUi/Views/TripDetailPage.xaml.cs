using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class TripDetailPage : ContentPage
{
    private readonly TripDetailViewModel _viewModel;

    public TripDetailPage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<TripDetailViewModel>(serviceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTripAsync();
    }
}
