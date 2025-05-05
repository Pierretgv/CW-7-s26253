namespace TravelAgencyAPI.Exceptions

public class ClientTripRegistrationException : Exception
{
    public ClientTripRegistrationException(int clientId, int tripId)
        : base($"Client with ID {clientId} could not be registered for trip with ID {tripId}.")
    {
    }
}