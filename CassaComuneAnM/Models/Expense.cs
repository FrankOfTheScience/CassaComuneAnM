namespace CassaComuneAnM.Models;
public class Expense
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool TourLeaderFree { get; set; }
    public List<string> Beneficiaries { get; set; } = new();

    public Expense(DateTime date, string description, decimal amount, bool tourLeaderFree, List<string> beneficiaries)
    {
        Date = date;
        Description = description;
        Amount = amount;
        TourLeaderFree = tourLeaderFree;
        Beneficiaries = beneficiaries;
    }
}