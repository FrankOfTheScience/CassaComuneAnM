namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ExpenseHistoryItemViewModel
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string BeneficiariesText { get; init; } = string.Empty;
}
