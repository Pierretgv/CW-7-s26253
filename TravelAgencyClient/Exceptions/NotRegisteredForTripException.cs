namespace TravelAgencyAPI.Exceptions

public class NotRegisteredForTripException : Exception
{
    public NotRegisteredForTripException(int clientId, int tripId)
        : base($"Client with ID {clientId} is not registered for trip with ID {tripId}.")
    {
    }
}