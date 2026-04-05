using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class DepositPage : ContentPage
{
    private readonly DepositViewModel _viewModel;

    public DepositPage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<DepositViewModel>(serviceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
