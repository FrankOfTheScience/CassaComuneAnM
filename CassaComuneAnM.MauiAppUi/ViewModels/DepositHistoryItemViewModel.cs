namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class DepositHistoryItemViewModel
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public string PayerName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal RemainingBudget { get; init; }
}
