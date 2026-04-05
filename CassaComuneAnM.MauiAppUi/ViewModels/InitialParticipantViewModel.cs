namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class InitialParticipantViewModel : BaseViewModel
{
    private string _name = string.Empty;
    private decimal _personalBudget;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal PersonalBudget
    {
        get => _personalBudget;
        set => SetProperty(ref _personalBudget, value);
    }
}
