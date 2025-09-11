using CassaComuneAnM.Models;

namespace CassaComuneAnM.Services.Interfaces;
public interface ITripService
{
    public List<Trip> GetAllTrips();
    public void SaveAllTrips(List<Trip> trips);
    public void SaveOrUpdateTrip(Trip trip);
    public void AddNewTrip(Trip trip);
    public void DeleteTrip(string tripCode);
}
