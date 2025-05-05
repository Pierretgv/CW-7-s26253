using Microsoft.Data.SqlClient;
using System.Data;
using TravelAgency.Exceptions;
using TravelAgency.Models;
using TravelAgency.Models.DTOs;

namespace TravelAgency.Services;

public interface IDbService
{
    Task<IEnumerable<TripGetDTO>> GetAllTripsAsync();
    Task<IEnumerable<TripGetDTO>> GetTripsByClientIdAsync(int clientId);
    Task<int> CreateClientAsync(ClientCreateDTO client);
    Task RegisterClientToTripAsync(int clientId, int tripId);
    Task UnregisterClientFromTripAsync(int clientId, int tripId);
}

public class DbService : IDbService
{
    private readonly string? _connectionString;

    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<IEnumerable<TripGetDTO>> GetAllTripsAsync()
    {
        var result = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sqlTrips = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS Country
            FROM Trip t
            LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
        ";

        await using var command = new SqlCommand(sqlTrips, connection);
        var reader = await command.ExecuteReaderAsync();

        var tripDict = new Dictionary<int, TripGetDTO>();

        while (await reader.ReadAsync())
        {
            var idTrip = reader.GetInt32(0);
            if (!tripDict.ContainsKey(idTrip))
            {
                tripDict[idTrip] = new TripGetDTO
                {
                    IdTrip = idTrip,
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = new List<string>()
                };
            }

            if (!reader.IsDBNull(6))
                tripDict[idTrip].Countries.Add(reader.GetString(6));
        }

        return tripDict.Values;
    }

    public async Task<IEnumerable<TripGetDTO>> GetTripsByClientIdAsync(int clientId)
    {
        var trips = new List<TripGetDTO>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string checkClient = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
        await using (var checkCommand = new SqlCommand(checkClient, connection))
        {
            checkCommand.Parameters.AddWithValue("@IdClient", clientId);
            var exists = await checkCommand.ExecuteScalarAsync();
            if (exists is null)
                throw new NotFoundException($"Client with id {clientId} does not exist");
        }

        const string sql = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
            FROM Trip t
            -- Możesz tu dodać inne warunki, jeśli wycieczki są powiązane z klientem w inny sposób
        ";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(new TripGetDTO
            {
                IdTrip = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5)
            });
        }

        return trips;
    }

    public async Task<int> CreateClientAsync(ClientCreateDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email))
            throw new BadRequestException("Missing required client fields");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
            SELECT SCOPE_IDENTITY()
        ";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", (object?)client.Telephone ?? DBNull.Value);
        command.Parameters.AddWithValue("@Pesel", (object?)client.Pesel ?? DBNull.Value);

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return id;
    }

    public async Task RegisterClientToTripAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string checkClient = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
        await using (var cmd = new SqlCommand(checkClient, connection))
        {
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            if (await cmd.ExecuteScalarAsync() is null)
                throw new NotFoundException("Client not found");
        }

        const string checkTrip = "SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
        int maxPeople;
        await using (var cmd = new SqlCommand(checkTrip, connection))
        {
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            var result = await cmd.ExecuteScalarAsync();
            if (result is null)
                throw new NotFoundException("Trip not found");
            maxPeople = (int)result;
        }

        const string currentCount = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
        await using (var cmd = new SqlCommand(currentCount, connection))
        {
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            var count = (int)await cmd.ExecuteScalarAsync();
            if (count >= maxPeople)
                throw new ConflictException("Trip is full");
        }

        const string insert =
            "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES (@IdClient, @IdTrip, @RegisteredAt)";
        await using var insertCmd = new SqlCommand(insert, connection);
        insertCmd.Parameters.AddWithValue("@IdClient", clientId);
        insertCmd.Parameters.AddWithValue("@IdTrip", tripId);
        insertCmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now.ToString("yyyyMMdd"));
        await insertCmd.ExecuteNonQueryAsync();
    }

    public async Task UnregisterClientFromTripAsync(int clientId, int tripId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string check = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        await using (var cmd = new SqlCommand(check, connection))
        {
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            if (await cmd.ExecuteScalarAsync() is null)
                throw new NotFoundException("Client is not registered for this trip");
        }

        const string delete = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        await using var deleteCmd = new SqlCommand(delete, connection);
        deleteCmd.Parameters.AddWithValue("@IdClient", clientId);
        deleteCmd.Parameters.AddWithValue("@IdTrip", tripId);
        await deleteCmd.ExecuteNonQueryAsync();
    }
}
