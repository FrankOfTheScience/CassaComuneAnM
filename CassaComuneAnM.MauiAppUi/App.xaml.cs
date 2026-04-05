using Microsoft.Extensions.DependencyInjection;

namespace CassaComuneAnM.MauiAppUi;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();
        MainPage = new NavigationPage(services.GetRequiredService<MainPage>());
    }
}
