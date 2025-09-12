namespace CassaComuneAnM.Core.Entities;
public class Participant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Balance { get; set; } = 0m;
    public required decimal PersonalBudget { get; set; }

    // Relazioni
    public List<Deposit> Deposits { get; set; } = new();

    public required int TripId { get; set; }
    public required Trip Trip { get; set; }

    // Many-to-many con Expense tramite ExpenseParticipant
    public ICollection<ExpenseParticipant> ExpenseParticipants { get; set; } = new List<ExpenseParticipant>();

    // Proprietà calcolata
    public decimal TotalPaid => Deposits.Sum(p => p.Amount);

    public Participant() { }
}
