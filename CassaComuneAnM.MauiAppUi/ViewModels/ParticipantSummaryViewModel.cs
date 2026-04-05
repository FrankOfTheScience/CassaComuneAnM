namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ParticipantSummaryViewModel
{
    public string Name { get; init; } = string.Empty;
    public decimal PersonalBudgetInEur { get; init; }
    public string PersonalBudgetPrimaryDisplay { get; init; } = string.Empty;
    public string PersonalBudgetSecondaryDisplay { get; init; } = string.Empty;
    public decimal TotalPaidInEur { get; init; }
    public string TotalPaidPrimaryDisplay { get; init; } = string.Empty;
    public string TotalPaidSecondaryDisplay { get; init; } = string.Empty;
    public decimal RemainingBudgetInEur { get; init; }
    public string RemainingBudgetPrimaryDisplay { get; init; } = string.Empty;
    public string RemainingBudgetSecondaryDisplay { get; init; } = string.Empty;
}
