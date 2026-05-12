using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class TripListPage : ContentPage, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly TripListViewModel _viewModel;
    private bool _disposed;

    public TripListPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _scope = serviceProvider.CreateScope();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<TripListViewModel>(_scope.ServiceProvider);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTripsAsync();
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
