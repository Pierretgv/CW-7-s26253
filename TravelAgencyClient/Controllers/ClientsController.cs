using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Models;
using TravelAgency.Models.DTOs;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ClientsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientCreateDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            string.IsNullOrWhiteSpace(client.Email))
        {
            return BadRequest("All fields are required.");
        }

        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(client.Email, emailRegex))
        {
            return BadRequest("Invalid email format.");
        }

        var id = await _dbService.CreateClientAsync(client);
        return CreatedAtAction(nameof(GetClientTrips), new { id }, new { IdClient = id });
    }

    [HttpPost("{id}/trips")]
    public async Task<IActionResult> EnrollClientOnTrip([FromRoute] int id, [FromBody] ClientTripCreateDTO body)
    {
        try
        {
            await _dbService.RegisterClientToTripAsync(id, body.TripId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient([FromRoute] int id)
    {
        try
        {
            await _dbService.DeleteClientAsync(id);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips([FromRoute] int id)
    {
        try
        {
            var trips = await _dbService.GetTripsByClientIdAsync(id);
            return Ok(trips);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
