using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class TripDetailPage : ContentPage, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly TripDetailViewModel _viewModel;
    private bool _disposed;

    public TripDetailPage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        _scope = serviceProvider.CreateScope();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<TripDetailViewModel>(_scope.ServiceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.LoadTripAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Errore", ex.Message, "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (Navigation?.NavigationStack.Contains(this) != true)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scope.Dispose();
        _disposed = true;
    }
}
