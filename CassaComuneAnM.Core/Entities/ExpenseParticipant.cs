namespace CassaComuneAnM.Core.Entities;
public class ExpenseParticipant
{
    public int ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;
}
