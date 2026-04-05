namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ParticipantSummaryViewModel
{
    public string Name { get; init; } = string.Empty;
    public decimal PersonalBudget { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal RemainingBudget { get; init; }
}
