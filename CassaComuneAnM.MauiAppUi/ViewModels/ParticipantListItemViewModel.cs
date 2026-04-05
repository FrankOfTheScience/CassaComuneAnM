namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ParticipantListItemViewModel
{
    public string Name { get; init; } = string.Empty;
    public decimal BudgetInEur { get; init; }
    public string BudgetPrimaryDisplay { get; init; } = string.Empty;
    public string BudgetSecondaryDisplay { get; init; } = string.Empty;
    public string ParticipantName { get; init; } = string.Empty;
}
