namespace CassaComuneAnM.MauiAppUi;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}
