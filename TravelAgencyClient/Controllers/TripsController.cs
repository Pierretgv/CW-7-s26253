using Microsoft.AspNetCore.Mvc;
using TravelAgency.Exceptions;
using TravelAgency.Models.DTOs;
using TravelAgency.Services;

namespace TravelAgency.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{
    private readonly IDbService _dbService;

    public TripsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        var trips = await _dbService.GetAllTripsAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTripById([FromRoute] int id)
    {
        try
        {
            var trip = await _dbService.GetTripByIdAsync(id);
            return Ok(trip);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrip([FromBody] TripCreateDTO body)
    {
        try
        {
            var createdTrip = await _dbService.CreateTripAsync(body);
            return CreatedAtAction(nameof(GetTripById), new { id = createdTrip.IdTrip }, createdTrip);
        }
        catch (BadRequestException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrip([FromRoute] int id)
    {
        try
        {
            await _dbService.DeleteTripAsync(id);
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
}