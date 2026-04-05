namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class SelectableParticipantViewModel : BaseViewModel
{
    private bool _isSelected;

    public string Name { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
