using CassaComuneAnM.MauiAppUi.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    public ICommand ShowTripsCommand { get; }
    public ICommand CreateTripCommand { get; }

    public MainPageViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Cassa Comune";
        ShowTripsCommand = new Command(async () => await OpenTripListAsync());
        CreateTripCommand = new Command(async () => await OpenCreateTripAsync());
    }

    private async Task OpenTripListAsync()
    {
        var page = _serviceProvider.GetRequiredService<TripListPage>();
        await Navigation!.PushAsync(page);
    }

    private async Task OpenCreateTripAsync()
    {
        var page = _serviceProvider.GetRequiredService<CreateTripPage>();
        await Navigation!.PushAsync(page);
    }
}
