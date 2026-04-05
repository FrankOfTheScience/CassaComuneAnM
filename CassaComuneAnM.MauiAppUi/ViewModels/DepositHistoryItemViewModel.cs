namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class DepositHistoryItemViewModel
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public string PayerName { get; init; } = string.Empty;
    public decimal AmountInEur { get; init; }
    public string AmountPrimaryDisplay { get; init; } = string.Empty;
    public string AmountSecondaryDisplay { get; init; } = string.Empty;
    public decimal RemainingBudgetInEur { get; init; }
    public string RemainingBudgetPrimaryDisplay { get; init; } = string.Empty;
    public string RemainingBudgetSecondaryDisplay { get; init; } = string.Empty;
}
