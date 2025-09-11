using CassaComuneAnM.Models;
using Spectre.Console;

namespace CassaComuneAnM.Services;
public static class ConsoleService
{
    // === CREAZIONE VIAGGIO ===
    public static void CreaNuovoViaggio(TripService tripService)
    {
        AnsiConsole.Clear();

        var trip = new Trip
        {
            TripName = AnsiConsole.Ask<string>("Nome viaggio?"),
            TripCode = AnsiConsole.Ask<string>("Codice viaggio?"),
            TripDate = AnsiConsole.Ask<DateTime>("Data del viaggio (yyyy-MM-dd)?"),
            CoordinatorName = AnsiConsole.Ask<string>("Nome coordinatore?"),
            CoordinatorCode = AnsiConsole.Ask<string>("Codice coordinatore?"),
            CashierName = AnsiConsole.Ask<string>("Nome cassiere?"),
            Country = AnsiConsole.Ask<string>("Paese?"),
            Currency = AnsiConsole.Ask<string>("Valuta?"),
            ExchangeRate = AnsiConsole.Ask<decimal>("Tasso di cambio?")
        };

        int numParticipants = AnsiConsole.Ask<int>("Numero partecipanti?");
        for (int i = 0; i < numParticipants; i++)
        {
            var name = AnsiConsole.Ask<string>($"Nome partecipante {i + 1}?");
            var budget = AnsiConsole.Ask<decimal>($"Budget personale per {name}?");
            trip.Participants.Add(new Participant(name, budget));
        }

        tripService.AddNewTrip(trip);
        AnsiConsole.MarkupLine($"[green]Viaggio '{trip.TripName}' creato con successo![/]");
        Pause();
    }

    // === RECUPERO VIAGGIO ===
    public static void RecuperaViaggio(TripService tripService)
    {
        AnsiConsole.Clear();
        var trips = tripService.GetAllTrips();

        if (!trips.Any())
        {
            AnsiConsole.MarkupLine("[red]Nessun viaggio trovato.[/]");
            Pause();
            return;
        }

        var scelta = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleziona un viaggio")
                .AddChoices(trips.Select(t => $"{t.TripName} ({t.TripCode})")));

        var trip = trips.First(t => $"{t.TripName} ({t.TripCode})" == scelta);

        MenuViaggio(tripService, trip);
    }

    // === MENU VIAGGIO ===
    public static void MenuViaggio(TripService tripService, Trip trip)
    {
        while (true)
        {
            AnsiConsole.Clear();

            var scelta = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]Gestione viaggio: {trip.TripName} ({trip.TripCode})[/]")
                    .AddChoices(new[] {
                        "Mostra dettagli",
                        "Gestisci partecipanti",
                        "Gestisci spese",
                        "Gestisci versamenti",
                        "Elimina viaggio",
                        "Torna al menu principale"
                    }));

            switch (scelta)
            {
                case "Mostra dettagli":
                    MostraDettagliViaggio(trip);
                    Pause();
                    break;
                case "Gestisci partecipanti":
                    GestisciPartecipanti(tripService, trip);
                    break;
                case "Gestisci spese":
                    GestisciSpese(tripService, trip);
                    break;
                case "Gestisci versamenti":
                    GestisciVersamenti(tripService, trip);
                    break;
                case "Elimina viaggio":
                    bool conferma = AnsiConsole.Confirm($"Sei sicuro di voler eliminare il viaggio '{trip.TripName}'?");
                    if (conferma)
                    {
                        tripService.DeleteTrip(trip.TripCode);
                        AnsiConsole.MarkupLine($"[red]Viaggio '{trip.TripName}' eliminato![/]");
                        Pause();
                        return; // torna al menu principale
                    }
                    break;
                case "Torna al menu principale":
                    return;
            }
        }
    }

    // === DETTAGLI VIAGGIO ===
    public static void MostraDettagliViaggio(Trip trip)
    {
        AnsiConsole.Clear();

        var table = new Table();
        table.AddColumn("Campo");
        table.AddColumn("Valore");

        table.AddRow("Nome", trip.TripName);
        table.AddRow("Codice", trip.TripCode);
        table.AddRow("Data", trip.TripDate.ToShortDateString());
        table.AddRow("Coordinatore", $"{trip.CoordinatorName} ({trip.CoordinatorCode})");
        table.AddRow("Cassiere", trip.CashierName);
        table.AddRow("Paese", trip.Country);
        table.AddRow("Valuta", trip.Currency);
        table.AddRow($"Tasso di cambio (Quanto vale 1 Euro?)", trip.ExchangeRate.ToString());
        table.AddRow("Budget Totale", trip.TotalBudget.ToString("F2"));
        table.AddRow("Totale Versato", trip.TotalPaid.ToString("F2"));
        table.AddRow("Totale Spese", trip.TotalExpenses.ToString("F2"));
        table.AddRow("Saldo Cassa", trip.CashBalance.ToString("F2"));

        AnsiConsole.Write(table);

        // Dettagli partecipanti
        var table2 = new Table().Border(TableBorder.Rounded);
        table2.AddColumn("Partecipante");
        table2.AddColumn("Budget personale");
        table2.AddColumn("Versato");
        table2.AddColumn("Residuo");

        foreach (var p in trip.Participants)
        {
            var versato = trip.Deposits.Where(d => d.PayerName == p.Name).Sum(d => d.Amount);
            var residuo = p.PersonalBudget - versato;

            table2.AddRow(
                p.Name,
                p.PersonalBudget.ToString("F2"),
                versato.ToString("F2"),
                residuo.ToString("F2"));
        }

        AnsiConsole.Write(table2);
    }

    // === GESTIONE PARTECIPANTI ===
    public static void GestisciPartecipanti(TripService tripService, Trip trip)
    {
        AnsiConsole.Clear();

        var scelta = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Gestione partecipanti")
                .AddChoices(new[] { "Aggiungi partecipante", "Rimuovi partecipante", "Indietro" }));

        switch (scelta)
        {
            case "Aggiungi partecipante":
                var name = AnsiConsole.Ask<string>("Nome partecipante?");
                var budget = AnsiConsole.Ask<decimal>($"Budget personale per {name}?");
                trip.Participants.Add(new Participant(name, budget));
                tripService.SaveOrUpdateTrip(trip);
                AnsiConsole.MarkupLine($"[green]Partecipante {name} aggiunto![/]");
                Pause();
                break;

            case "Rimuovi partecipante":
                if (!trip.Participants.Any())
                {
                    AnsiConsole.MarkupLine("[red]Nessun partecipante da rimuovere.[/]");
                    Pause();
                    return;
                }
                var nameToRemove = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Seleziona partecipante da rimuovere")
                        .AddChoices(trip.Participants.Select(p => p.Name)));
                var participant = trip.Participants.First(p => p.Name == nameToRemove);
                trip.Participants.Remove(participant);
                tripService.SaveOrUpdateTrip(trip);
                AnsiConsole.MarkupLine($"[red]Partecipante {nameToRemove} rimosso![/]");
                Pause();
                break;

            case "Indietro":
                return;
        }
    }

    // === GESTIONE SPESE ===
    public static void GestisciSpese(TripService tripService, Trip trip)
    {
        AnsiConsole.Clear();

        var scelta = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Gestione spese")
                .AddChoices(new[] { "Aggiungi spesa", "Mostra spese", "Indietro" }));

        switch (scelta)
        {
            case "Aggiungi spesa":
                AggiungiSpesa(tripService, trip);
                break;
            case "Mostra spese":
                MostraSpese(trip);
                Pause();
                break;
            case "Indietro":
                return;
        }
    }

    // === GESTIONE VERSAMENTI ===
    public static void GestisciVersamenti(TripService tripService, Trip trip)
    {
        AnsiConsole.Clear();

        var scelta = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Gestione versamenti")
                .AddChoices(new[] { "Aggiungi versamento", "Mostra versamenti", "Situazione cassa", "Indietro" }));

        switch (scelta)
        {
            case "Aggiungi versamento":
                AggiungiVersamento(tripService, trip);
                break;

            case "Mostra versamenti":
                MostraVersamenti(trip);
                Pause();
                break;

            case "Situazione cassa":
                MostraSituazioneCassa(trip);
                Pause();
                break;

            case "Indietro":
                return;
        }
    }

    // === METODI AUSILIARI PER VERSAMENTI E SPESE ===
    public static void AggiungiVersamento(TripService tripService, Trip trip)
    {
        var payer = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Seleziona partecipante che versa")
                .AddChoices(trip.Participants.Select(p => p.Name)));

        var participant = trip.Participants.First(p => p.Name == payer);
        var date = AnsiConsole.Ask<DateTime>("Data versamento (yyyy-MM-dd)?");

        decimal amount;
        while (true)
        {
            amount = AnsiConsole.Ask<decimal>("Importo versato?");
            if (amount <= 0)
            {
                AnsiConsole.MarkupLine("[red]L'importo deve essere maggiore di zero.[/]");
                continue;
            }

            decimal giaVersato = trip.Deposits.Where(d => d.PayerName == participant.Name).Sum(d => d.Amount);
            decimal residuo = participant.PersonalBudget - giaVersato;

            if (amount > residuo)
            {
                bool aumenta = AnsiConsole.Confirm(
                    $"L'importo supera il residuo di budget per {participant.Name} ({residuo:F2}). " +
                    "Vuoi aumentare il budget per tutti i partecipanti?");

                if (aumenta)
                {
                    decimal delta = amount - residuo;
                    foreach (var p in trip.Participants)
                        p.PersonalBudget += delta;

                    AnsiConsole.MarkupLine(
                        $"[green]Budget aumentato di {delta:F2} per ciascun partecipante. " +
                        $"Nuovo budget personale: {participant.PersonalBudget:F2}[/]");
                }
                else
                {
                    continue; // torna a chiedere l'importo
                }
            }

            break;
        }

        trip.Deposits.Add(new Deposit(date, payer, amount));
        tripService.SaveOrUpdateTrip(trip);
        AnsiConsole.MarkupLine("[green]Versamento registrato![/]");
        Pause();
    }

    public static void MostraVersamenti(Trip trip)
    {
        if (!trip.Deposits.Any())
        {
            AnsiConsole.MarkupLine("[red]Nessun versamento registrato.[/]");
            return;
        }
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Data");
        table.AddColumn("Partecipante");
        table.AddColumn("Importo");
        foreach (var v in trip.Deposits)
            table.AddRow(v.Date.ToShortDateString(), v.PayerName, v.Amount.ToString("F2"));
        AnsiConsole.Write(table);
    }

    public static void MostraSituazioneCassa(Trip trip)
    {
        AnsiConsole.MarkupLine("[underline green]Situazione cassa del viaggio[/]");

        // Dettaglio partecipanti
        var detailTable = new Table().Border(TableBorder.Rounded);
        detailTable.AddColumn("Partecipante");
        detailTable.AddColumn("Budget personale");
        detailTable.AddColumn("Versato");
        detailTable.AddColumn("Residuo");

        foreach (var p in trip.Participants)
        {
            var versato = trip.Deposits.Where(d => d.PayerName == p.Name).Sum(d => d.Amount);
            var residuo = p.PersonalBudget - versato;

            detailTable.AddRow(
                p.Name,
                p.PersonalBudget.ToString("F2"),
                versato.ToString("F2"),
                residuo.ToString("F2"));
        }

        AnsiConsole.Write(detailTable);

        // Riepilogo
        var summary = new Table().AddColumn("").AddColumn("");
        summary.AddRow("Budget totale", trip.TotalBudget.ToString("F2"));
        summary.AddRow("Totale versato", trip.TotalPaid.ToString("F2"));
        summary.AddRow("Totale spese", trip.TotalExpenses.ToString("F2"));
        summary.AddRow("Saldo cassa", trip.CashBalance.ToString("F2"));
        AnsiConsole.Write(summary);
    }

    public static void AggiungiSpesa(TripService tripService, Trip trip)
    {
        var date = AnsiConsole.Ask<DateTime>("Data spesa (yyyy-MM-dd)?");
        var description = AnsiConsole.Ask<string>("Descrizione?");
        var amount = AnsiConsole.Ask<decimal>("Importo totale?");
        var tourLeaderFree = AnsiConsole.Confirm("Tour Leader Free?");

        var allParticipants = trip.Participants.Select(p => p.Name).ToList();
        var isForAll = AnsiConsole.Confirm("La spesa riguarda tutto il gruppo?");
        List<string> excluded = new();
        if (!isForAll)
        {
            excluded = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Seleziona chi NON partecipa")
                    .NotRequired()
                    .PageSize(10)
                    .AddChoices(allParticipants));
        }

        var beneficiaries = allParticipants.Except(excluded).ToList();
        int totalPeople = allParticipants.Count;
        int payersCount = beneficiaries.Count;
        decimal costPerPerson = amount / totalPeople;
        if (tourLeaderFree && beneficiaries.Contains(trip.CoordinatorName))
        {
            payersCount--;
            description += $" (TL Free of {costPerPerson:F2})";
        }
        decimal effectiveTotal = costPerPerson * (totalPeople - (tourLeaderFree ? 1 : 0));
        decimal costPerPayer = effectiveTotal / (payersCount > 0 ? payersCount : 1);

        var expense = new Expense(date, description, effectiveTotal, tourLeaderFree, beneficiaries);
        trip.Expenses.Add(expense);

        foreach (var ex in excluded)
        {
            var refund = tourLeaderFree ? costPerPayer : costPerPerson;
            var refundExpense = new Expense(date, $"Rimborso {description} ({ex})", refund, false, new List<string> { ex });
            trip.Expenses.Add(refundExpense);
        }

        tripService.SaveOrUpdateTrip(trip);
        AnsiConsole.MarkupLine($"[green]Spesa '{description}' registrata con successo![/]");
        Pause();
    }

    public static void MostraSpese(Trip trip)
    {
        if (!trip.Expenses.Any())
        {
            AnsiConsole.MarkupLine("[red]Nessuna spesa registrata.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Data");
        table.AddColumn("Descrizione");
        table.AddColumn("Importo");
        table.AddColumn("Beneficiari");

        foreach (var e in trip.Expenses)
        {
            table.AddRow(
                e.Date.ToShortDateString(),
                e.Description,
                e.Amount.ToString("F2"),
                string.Join(", ", e.Beneficiaries));
        }

        AnsiConsole.Write(table);
    }

    // === METODO AUSILIARIO ===
    private static void Pause()
    {
        AnsiConsole.MarkupLine("\nPremi un tasto per continuare...");
        Console.ReadKey(true);
    }
}
