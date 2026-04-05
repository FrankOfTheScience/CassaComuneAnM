using CassaComuneAnM.Core.Entities;
using CassaComuneAnM.Core.Enums;
using System.Globalization;

namespace CassaComuneAnM.MauiAppUi.Services;

public static class CurrencyDisplayService
{
    public static CurrencyCode GetTripCurrency(Trip trip)
    {
        return Enum.TryParse<CurrencyCode>(trip.Currency, true, out var currency)
            ? currency
            : CurrencyCode.EUR;
    }

    public static bool IsTripCurrencyEuro(Trip trip) => GetTripCurrency(trip) == CurrencyCode.EUR;

    public static decimal ConvertEurToTripCurrency(decimal amountInEur, Trip trip)
    {
        return IsTripCurrencyEuro(trip) ? amountInEur : amountInEur * NormalizeExchangeRate(trip);
    }

    public static decimal ConvertTripCurrencyToEur(decimal amountInTripCurrency, Trip trip)
    {
        return IsTripCurrencyEuro(trip) ? amountInTripCurrency : amountInTripCurrency / NormalizeExchangeRate(trip);
    }

    public static decimal ConvertInputToEur(decimal amount, CurrencyCode inputCurrency, Trip trip)
    {
        return inputCurrency == CurrencyCode.EUR
            ? amount
            : ConvertTripCurrencyToEur(amount, trip);
    }

    public static string FormatAmountWithEur(decimal amountInEur, Trip trip)
    {
        var tripCurrency = GetTripCurrency(trip);
        if (tripCurrency == CurrencyCode.EUR)
        {
            return $"EUR {FormatNumber(amountInEur)}";
        }

        var localAmount = ConvertEurToTripCurrency(amountInEur, trip);
        return $"{tripCurrency} {FormatNumber(localAmount)} · EUR {FormatNumber(amountInEur)}";
    }

    public static string FormatPrimaryAmount(decimal amountInEur, Trip trip)
    {
        var tripCurrency = GetTripCurrency(trip);
        if (tripCurrency == CurrencyCode.EUR)
        {
            return $"EUR {FormatNumber(amountInEur)}";
        }

        var localAmount = ConvertEurToTripCurrency(amountInEur, trip);
        return $"{tripCurrency} {FormatNumber(localAmount)}";
    }

    public static string FormatSecondaryEurAmount(decimal amountInEur, Trip trip)
    {
        return IsTripCurrencyEuro(trip)
            ? string.Empty
            : $"  EUR {FormatNumber(amountInEur)}";
    }

    public static string FormatInputModeLabel(CurrencyCode inputCurrency, Trip trip)
    {
        var tripCurrency = GetTripCurrency(trip);
        if (inputCurrency == CurrencyCode.EUR)
        {
            return "INSERIMENTO IN EUR";
        }

        return $"INSERIMENTO IN {tripCurrency}";
    }

    public static IReadOnlyList<CurrencyOption> GetInputOptions(Trip trip)
    {
        var tripCurrency = GetTripCurrency(trip);
        if (tripCurrency == CurrencyCode.EUR)
        {
            return new[] { new CurrencyOption(CurrencyCode.EUR, CurrencyCatalog.GetItalianName(CurrencyCode.EUR)) };
        }

        return new[]
        {
            new CurrencyOption(CurrencyCode.EUR, CurrencyCatalog.GetItalianName(CurrencyCode.EUR)),
            new CurrencyOption(tripCurrency, CurrencyCatalog.GetItalianName(tripCurrency))
        };
    }

    public static string BuildExchangeRateHelp(CurrencyCode currency)
    {
        return currency == CurrencyCode.EUR
            ? "CAMBIO CONTRO EUR. PER EUR LASCIA VUOTO OPPURE INSERISCI 1."
            : $"CAMBIO CONTRO EUR. ESEMPIO: {currency} 1,10 SIGNIFICA CHE 1 EUR VALE 1,10 {currency}.";
    }

    private static decimal NormalizeExchangeRate(Trip trip) => trip.ExchangeRate > 0 ? trip.ExchangeRate : 1m;

    private static string FormatNumber(decimal value)
    {
        return value.ToString("N2", CultureInfo.GetCultureInfo("it-IT"));
    }
}
