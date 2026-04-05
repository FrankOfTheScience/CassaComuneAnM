using CassaComuneAnM.MauiAppUi.ViewModels;

namespace CassaComuneAnM.MauiAppUi.Views;

public partial class CreateTripPage : ContentPage
{
    public CreateTripPage(CreateTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
