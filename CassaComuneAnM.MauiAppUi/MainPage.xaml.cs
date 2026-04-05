using CassaComuneAnM.MauiAppUi.ViewModels;

namespace CassaComuneAnM.MauiAppUi;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
