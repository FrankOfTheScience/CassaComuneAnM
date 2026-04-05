namespace CassaComuneAnM.MauiAppUi.ViewModels;

public class ExpenseHistoryItemViewModel
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal AmountInEur { get; init; }
    public string AmountPrimaryDisplay { get; init; } = string.Empty;
    public string AmountSecondaryDisplay { get; init; } = string.Empty;
    public string BeneficiariesText { get; init; } = string.Empty;
}
