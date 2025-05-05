namespace TravelAgencyAPI.Exceptions

public class TripMaxParticipantsExceededException : Exception
{
    public TripMaxParticipantsExceededException(int tripId)
        : base($"The maximum number of participants for trip with ID {tripId} has been exceeded.")
    {
    }
}