namespace TravelAgencyAPI.Exceptions

public class InvalidClientDataException : Exception
{
    public InvalidClientDataException(string message)
        : base($"Invalid client data: {message}")
    {
    }
}