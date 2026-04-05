using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class ParticipantPage : ContentPage
{
    private readonly ParticipantViewModel _viewModel;

    public ParticipantPage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<ParticipantViewModel>(serviceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
