# CassaComuneAnM 2.0 - Features

## 1. Gestione viaggi

- creazione viaggio con nome, codice, data, coordinatore, cassiere, paese, valuta e cambio
- modifica dei dati anagrafici del viaggio
- eliminazione viaggio con conferma
- overview rapida con budget totale, versato, spese e saldo cassa

## 2. Gestione valuta

- selezione valuta tramite currency code
- supporto a cambio rispetto a EUR
- inserimento di spese e versamenti sia in EUR sia nella valuta del viaggio
- conversione automatica
- visualizzazione del corrispettivo in EUR quando il viaggio usa una valuta diversa da EUR

## 3. Partecipanti

- aggiunta partecipanti
- budget personale opzionale con fallback al budget standard del viaggio
- rimozione partecipanti
- situazione cassa per partecipante con budget, versato e residuo
- filtri e ordinamenti nella lista partecipanti

## 4. Versamenti

- registrazione versamenti
- modifica ed eliminazione versamenti
- validazione sul residuo disponibile
- possibilità di aumento budget con conferma esplicita
- dettaglio singolo versamento da lista compatta
- filtri e ordinamenti nella lista versamenti

## 5. Spese

- registrazione spese
- modifica ed eliminazione spese
- esclusione partecipanti dalla spesa
- logica `Tour Leader Free`
- gestione dei beneficiari
- dettaglio singola spesa da lista compatta
- filtri e ordinamenti nella lista spese

## 6. Cassa e warning

- saldo cassa aggiornato automaticamente
- supporto a saldo negativo
- evidenziazione del disavanzo
- messaggi di warning quando una spesa porta la cassa sotto zero

## 7. UX/UI 2.0

- interfaccia MAUI responsive per phone e tablet
- stile custom coerente con palette bianco/verde/nero
- dialog custom per selezioni e dettagli
- date picker custom a mini calendario
- card compatte in lista e dettaglio separato

## 8. Qualità

- test automatici sui casi limite del service layer
- pipeline CI/CD su `master`
- semantic versioning automatico da conventional commits
- GitHub Release con changelog automatico e APK allegato

## 9. Limiti noti

- warning sicurezza ancora aperto su `AutoMapper 15.0.1`
- la build Android in CI dipende dai workload MAUI installati dall'action
- il publish attuale produce APK; eventuale firma di produzione può essere aggiunta in un passo successivo
