using CassaComuneAnM.Data.Interfaces;
using CassaComuneAnM.Models;
using System.Text.Json;

namespace CassaComuneAnM.Data;
public class TripRepository : ITripRepository
{
    private readonly string _filePath = "trips.json";

    public List<Trip> LoadTrips()
    {
        if (!File.Exists(_filePath))
            return new List<Trip>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Trip>>(json) ?? new List<Trip>();
    }

    public void SaveTrips(List<Trip> trips)
    {
        var json = JsonSerializer.Serialize(trips, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
