namespace CassaComuneAnM.Core.Entities;
public class Deposit
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public required string PayerName { get; set; }
    public required decimal Amount { get; set; }
    public required int ParticipantId { get; set; }
    public required Participant Participant { get; set; }
    public required int TripId { get; set; }
    public required Trip Trip { get; set; }

    public Deposit()
    {}

    public override string ToString() =>
        $"{PayerName} ha depositato {Amount:C} in data {Date:d}";
}
