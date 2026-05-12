using CassaComuneAnM.MauiAppUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class CreateTripPage : ContentPage, IDisposable
{
    private readonly IServiceScope _scope;
    private bool _disposed;

    public CreateTripPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _scope = serviceProvider.CreateScope();
        BindingContext = ActivatorUtilities.CreateInstance<CreateTripViewModel>(_scope.ServiceProvider);
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
