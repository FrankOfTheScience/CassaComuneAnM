namespace CassaComuneAnM.Core.Entities;
public class Trip
{
    public int Id { get; set; }
    public required string TripName { get; set; }
    public required string TripCode { get; set; }
    public DateTime TripDate { get; set; }
    public required string CoordinatorName { get; set; }
    public required string CoordinatorCode { get; set; }
    public required string CashierName { get; set; }
    public required string Country { get; set; }
    public required string Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal BudgetPerPax { get; set; }
    public List<Participant> Participants { get; set; } = new();
    public List<Expense> Expenses { get; set; } = new();
    public List<Deposit> Deposits { get; set; } = new();

    // Calcolated properties
    public decimal TotalBudget => Participants.Sum(p => p.PersonalBudget);
    public decimal TotalPaid => Deposits.Sum(p => p.Amount);
    public decimal TotalExpenses => Expenses.Sum(e => e.Amount);
    public decimal CashBalance => TotalPaid - TotalExpenses;
}
