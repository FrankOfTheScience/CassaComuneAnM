using CassaComuneAnM.Models;

namespace CassaComuneAnM.Data.Interfaces;
public interface ITripRepository
{
    public List<Trip> LoadTrips();
    public void SaveTrips(List<Trip> trips);
}
