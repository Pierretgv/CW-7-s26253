namespace TravelAgencyAPI.Exceptions

public class DatabaseConnectionException : Exception
{
    public DatabaseConnectionException(string message)
        : base($"Database connection error: {message}")
    {
    }
}