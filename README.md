# CassaComuneAnM 2.0

Frontend MAUI per la gestione della cassa comune di viaggio.

`CassaComuneAnM 2.0` sostituisce l'impostazione console-first della 1.x con un'app mobile/desktop orientata ai flussi operativi reali: viaggi, partecipanti, versamenti, spese, situazione cassa, doppia valuta e riepiloghi rapidi.

## Cosa fa

- crea e modifica viaggi con coordinatore, cassiere, paese, valuta e cambio
- gestisce partecipanti con budget standard o personalizzato
- registra versamenti in EUR o nella valuta del viaggio, con conversione automatica
- registra spese in EUR o nella valuta del viaggio, con esclusioni, ripartizione e logica `Tour Leader Free`
- mostra situazione cassa per partecipante e a livello viaggio
- evidenzia saldo cassa negativo e disavanzi
- include filtri, ordinamenti e viste compatte con dettaglio dedicato

## Stack

- `.NET 8`
- `.NET MAUI`
- `Entity Framework Core`
- `SQLite`
- `xUnit`

## Struttura repository

- [CassaComuneAnM.Core](C:/Users/fdell/source/repos/CassaComuneAnM/CassaComuneAnM.Core): entità e enum
- [CassaComuneAnm.Application](C:/Users/fdell/source/repos/CassaComuneAnM/CassaComuneAnm.Application): servizi applicativi e regole business
- [CassaComuneAnM.Infrastructure](C:/Users/fdell/source/repos/CassaComuneAnM/CassaComuneAnM.Infrastructure): EF Core, SQLite, repository
- [CassaComuneAnM.MauiAppUi](C:/Users/fdell/source/repos/CassaComuneAnM/CassaComuneAnM.MauiAppUi): frontend MAUI 2.0
- [CassaComuneAnM.Tests](C:/Users/fdell/source/repos/CassaComuneAnM/CassaComuneAnM.Tests): test automatici

## Avvio locale

Prerequisiti:

- `SDK .NET 8`
- workload MAUI per la piattaforma che vuoi eseguire
- Visual Studio 2022 oppure CLI `dotnet`

Comandi utili:

```powershell
dotnet build CassaComuneAnM.sln
dotnet test CassaComuneAnM.Tests\CassaComuneAnM.Tests.csproj
dotnet build CassaComuneAnM.MauiAppUi\CassaComuneAnM.MauiAppUi.csproj
```

## Release 2.0

La pipeline GitHub su `master` esegue:

- restore, build e test
- calcolo versione automatico da conventional commits
- creazione tag Git
- publish APK Android
- upload artifact
- creazione GitHub Release con changelog automatico e allegato APK

## Download applicazione

Per installare l'applicazione distribuita:

1. vai nella sezione [GitHub Releases](https://github.com/FrankOfTheScience/CassaComuneAnM/releases)
2. apri l'ultima release stabile
3. scarica l'asset disponibile:
   - `APK`, se vuoi installare l'app Android
   - eventuale `ZIP`, se la release contiene anche un pacchetto desktop
4. se hai scaricato uno `ZIP`, estrailo in una cartella locale
5. se hai scaricato un `APK`, trasferiscilo sul dispositivo Android e installalo

Note pratiche:

- su Android potresti dover abilitare temporaneamente l'installazione da sorgenti esterne
- su desktop, se la release contiene uno zip applicativo, apri la cartella estratta e avvia l'eseguibile incluso

## Branching

- `dev`: integrazione continua
- `master`: release branch

## Documentazione funzionale

Per il dettaglio completo dei flussi supportati consulta [FEATURES.md](C:/Users/fdell/source/repos/CassaComuneAnM/FEATURES.md).
