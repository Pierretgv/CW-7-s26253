namespace TravelAgencyAPI.Exceptions

public class TripNotFoundException : Exception
{
    public TripNotFoundException(int tripId)
        : base($"Trip with ID {tripId} not found.")
    {
    }
}