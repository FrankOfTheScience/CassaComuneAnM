using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class ExpensePage : ContentPage
{
    private readonly ExpenseViewModel _viewModel;

    public ExpensePage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<ExpenseViewModel>(serviceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
