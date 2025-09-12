using CassaComuneAnM.Core.Entities.Enum;

namespace CassaComuneAnM.Core.Entities;
public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool TourLeaderFree { get; set; }

    // Relazione molti-a-molti con Participant
    public ICollection<ExpenseParticipant> ExpenseParticipants { get; set; } = new List<ExpenseParticipant>();

    // Relazione uno-a-molti con Trip
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    // Categoria della spesa
    public Tag Category { get; set; }

    // Costruttore per comodità (non include ExpenseParticipants perché saranno gestiti a parte)
    public Expense(DateTime date, string description, decimal amount, bool tourLeaderFree, Tag category = Tag.Other)
    {
        Date = date;
        Description = description;
        Amount = amount;
        TourLeaderFree = tourLeaderFree;
        Category = category;
    }

    // Costruttore vuoto richiesto da EF Core
    public Expense() { }
}