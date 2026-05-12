using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class DepositPage : ContentPage, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly DepositViewModel _viewModel;
    private bool _disposed;

    public DepositPage(IServiceProvider serviceProvider, string tripCode)
    {
        InitializeComponent();
        _scope = serviceProvider.CreateScope();
        BindingContext = _viewModel = ActivatorUtilities.CreateInstance<DepositViewModel>(_scope.ServiceProvider, tripCode);
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
