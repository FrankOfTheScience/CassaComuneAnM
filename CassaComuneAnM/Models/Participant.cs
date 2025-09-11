namespace CassaComuneAnM.Models;
public class Participant
{
    public string Name { get; set; }
    public decimal Balance { get; set; } = 0m;
    public decimal PersonalBudget { get; set; }
    public List<Deposit> Deposits { get; set; } = new();

    public decimal TotalPaid => Deposits.Sum(p => p.Amount);

    public Participant(string name, decimal personalBudget)
    {
        Name = name;
        PersonalBudget = personalBudget;
    }
}
