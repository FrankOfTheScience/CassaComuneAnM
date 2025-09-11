using System.Text.Json;
using CassaComuneAnM.Models;
using CassaComuneAnM.Services.Interfaces;

namespace CassaComuneAnM.Services
{
    public class TripService : ITripService
    {
        private readonly string _filePath = "trips.json";

        // === OTTIENI TUTTI I VIAGGI ===
        public List<Trip> GetAllTrips()
        {
            if (!File.Exists(_filePath))
                return new List<Trip>();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Trip>();

            return JsonSerializer.Deserialize<List<Trip>>(json) ?? new List<Trip>();
        }

        // === SALVA TUTTI I VIAGGI (privato) ===
        public void SaveAllTrips(List<Trip> trips)
        {
            var json = JsonSerializer.Serialize(trips, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        // === CREA O AGGIORNA VIAGGIO ===
        public void SaveOrUpdateTrip(Trip trip)
        {
            var trips = GetAllTrips();

            var existing = trips.FirstOrDefault(t => t.TripCode == trip.TripCode);
            if (existing != null)
            {
                trips.Remove(existing); // rimuovi vecchia versione
            }

            trips.Add(trip); // aggiungi aggiornata
            SaveAllTrips(trips);
        }

        // === AGGIUNGI NUOVO VIAGGIO (se serve esplicitamente solo per creazione) ===
        public void AddNewTrip(Trip trip)
        {
            var trips = GetAllTrips();

            if (trips.Any(t => t.TripCode == trip.TripCode))
                throw new InvalidOperationException($"Esiste già un viaggio con codice {trip.TripCode}");

            trips.Add(trip);
            SaveAllTrips(trips);
        }

        // === ELIMINA VIAGGIO ===
        public void DeleteTrip(string tripCode)
        {
            var trips = GetAllTrips();
            var existing = trips.FirstOrDefault(t => t.TripCode == tripCode);
            if (existing != null)
            {
                trips.Remove(existing);
                var json = JsonSerializer.Serialize(trips, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
