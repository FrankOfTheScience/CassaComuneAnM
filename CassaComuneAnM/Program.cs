using CassaComuneAnM.Services;
using Spectre.Console;

var tripService = new TripService();

while (true)
{
    var scelta = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Menu principale[/]")
            .AddChoices(new[] {
                "Crea nuovo viaggio",
                "Recupera viaggio esistente",
                "Esci"
            }));

    switch (scelta)
    {
        case "Crea nuovo viaggio":
            ConsoleService.CreaNuovoViaggio(tripService);
            break;
        case "Recupera viaggio esistente":
            ConsoleService.RecuperaViaggio(tripService);
            break;
        case "Esci":
            return;
    }
}