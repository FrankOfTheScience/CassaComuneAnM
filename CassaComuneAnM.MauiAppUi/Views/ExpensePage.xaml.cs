using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class ExpensePage : ContentPage, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly ExpenseViewModel _viewModel;
    private bool _disposed;

    public ExpensePage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        _scope = serviceProvider.CreateScope();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<ExpenseViewModel>(_scope.ServiceProvider, tripCode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
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
