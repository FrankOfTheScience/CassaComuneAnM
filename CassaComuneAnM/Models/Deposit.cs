namespace CassaComuneAnM.Models;
public class Deposit
{
    public DateTime Date { get; set; }
    public string PayerName { get; set; }
    public decimal Amount { get; set; }

    public Deposit(DateTime date, string payerName, decimal amount)
    {
        Date = date;
        PayerName = payerName;
        Amount = amount;
    }

    public override string ToString() =>
        $"{PayerName} ha depositato {Amount:C} in data {Date:d}";
}
