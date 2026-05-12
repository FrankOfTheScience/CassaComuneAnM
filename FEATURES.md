# CassaComuneAnM 2.0 - Guida Funzionale

## Panoramica

L'app gestisce una cassa comune di viaggio partendo da un principio semplice:

- il budget viene definito in `EUR`
- la valuta del viaggio serve per l'operativita locale
- il sistema mantiene i calcoli coerenti e mostra, quando serve, sia il valore locale sia il corrispettivo in EUR

## 1. Impostazione del viaggio

Quando crei o modifichi un viaggio imposti:

- nome e codice viaggio
- data
- coordinatore
- cassiere
- paese
- valuta del viaggio
- tasso di cambio
- budget base per partecipante

Il `budget base per partecipante` e il budget standard che verra applicato ai nuovi partecipanti se non inserisci un budget personale specifico.

## 2. Cambio valuta

Il cambio e sempre espresso rispetto a `EUR`, che e la valuta di riferimento interna del sistema.

Esempio:

- valuta viaggio: `USD`
- cambio: `1,10`

Significa:

- `1 EUR = 1,10 USD`

Di conseguenza:

- se inserisci una spesa di `110 USD`, il sistema la converte in `100 EUR`
- se inserisci un versamento di `100 EUR`, il sistema puo mostrarti il corrispettivo locale di `110 USD`

Regola pratica:

- se il viaggio e in una valuta diversa da EUR, il tasso deve dire quanta valuta locale corrisponde a `1 EUR`

## 3. Budget, versamenti e residuo

Ogni partecipante ha:

- un `budget personale` espresso in EUR
- il totale `versato`
- il `residuo`, cioe quanto manca per coprire il proprio budget

Formula:

- `residuo = budget personale - totale versato`

Se lasci vuoto il budget personale del partecipante:

- viene usato il budget standard del viaggio

Quando registri un versamento:

- puoi inserirlo in `EUR`
- oppure nella valuta del viaggio
- il sistema converte e salva il valore coerente con i calcoli della cassa

Se un versamento supera il residuo disponibile del partecipante:

- l'app propone di aumentare il budget
- se confermi, l'aumento viene esteso a tutti i partecipanti

## 4. Come vengono calcolati i totali della cassa

A livello viaggio:

- `budget totale = somma dei budget personali dei partecipanti`
- `totale versato = somma di tutti i versamenti`
- `totale spese = somma di tutte le spese`
- `saldo cassa = totale versato - totale spese`

A livello partecipante:

- `totale versato partecipante = somma dei suoi versamenti`
- `residuo partecipante = budget personale - totale versato partecipante`

Nota importante:

- il residuo del partecipante non e il suo saldo finale di viaggio
- e solo quanto manca rispetto al budget assegnato

## 5. Spese ed esclusione di partecipanti

Quando registri una spesa puoi escludere uno o piu partecipanti dalla spesa.

Significa:

- la spesa non viene considerata a carico di tutti
- i partecipanti esclusi non rientrano tra i beneficiari

Effetto pratico:

- il sistema costruisce la spesa principale sui partecipanti alla spesa effettivi
- genera automaticamente un importo di rimborso per chi non ha partecipato
- mantiene la ripartizione corretta tra chi ha effettivamente beneficiato della spesa

In altre parole:

- escludere uno o piu partecipanti modifica il gruppo che beneficia della spesa
- chi non partecipa riceve un rimborso o un credito interno corrispondente
- i partecipanti paganti dividono il costo effettivo tra loro

## 6. Logica Tour Leader Free

La modalita `Tour Leader Free` serve quando il coordinatore ottiene una gratuità per un servizio di terra, ma la spesa è condivisa tra tutti.

Esempio pratico:

* partecipanti: 9 + 1 coordinatore
* costo di un biglietto: 2 EUR a testa → totale 20 EUR
* Tour Leader Free attivo: il coordinatore non paga la sua quota
* totale reale da pagare: 18 EUR
* la spesa totale viene divisa tra tutti i 10 partecipanti, quindi: 18 ÷ 10 = 1,8 EUR a testa

Effetto:

* il coordinatore usufruisce della spesa gratuitamente
* il risparmio del coordinatore viene ripartito su tutti i partecipanti
* ogni partecipante paga una quota leggermente inferiore rispetto al costo standard

Questa logica si applica a qualsiasi tipo di spesa dove il coordinatore deve usufruire ma non contribuire economicamente, ad esempio:

- hotel
- ingressi
- transfer
- pasti condivisi

in cui il tour leader partecipa ma non deve sostenere la spesa.

## 7. Perche la cassa puo andare in negativo

La cassa puo andare in negativo volutamente.

Scenario tipico:

- il cassiere registra subito una spesa pagata o anticipata da qualcuno
- i versamenti vengono registrati dopo

Questo comportamento e supportato per non perdere traccia delle spese reali.

Formula:

- se `totale spese > totale versato`, allora il `saldo cassa` va in negativo

Quando succede:

- l'app evidenzia il disavanzo
- il saldo viene mostrato come anomalia da coprire

Cosa si dovrebbe fare dopo:

- registrare i versamenti mancanti
- oppure coprire il disavanzo con una nuova entrata

L'obiettivo e riportare il saldo verso `0` o in positivo.

## 8. Liste e dettagli

Le liste sono volutamente compatte:

- mostrano solo le informazioni essenziali
- il dettaglio completo si apre con il pulsante dedicato

Nel dettaglio puoi trovare:

- tutti i dati principali dell'elemento
- azioni come modifica o eliminazione, quando previste

## 9. Filtri e ordinamenti

Le liste principali supportano:

- filtro testuale
- ordinamento per i campi piu rilevanti
- direzione `ASC` o `DESC`

Questo vale per:

- viaggi
- partecipanti
- versamenti
- spese
- situazione cassa per partecipante

## 10. Release e distribuzione

La 2.x e pensata per essere distribuita da GitHub Releases.

La pipeline su `master` esegue:

- restore
- build
- test
- versionamento automatico
- tag Git
- publish Android APK
- upload artifact
- GitHub Release con changelog automatico

## 11. Limiti noti

- warning sicurezza ancora aperto su `AutoMapper 15.0.1`
- la build Android in CI dipende dai workload MAUI installati dalla GitHub Action
- il publish attuale genera APK; l'eventuale firma di produzione puo essere aggiunta successivamente
